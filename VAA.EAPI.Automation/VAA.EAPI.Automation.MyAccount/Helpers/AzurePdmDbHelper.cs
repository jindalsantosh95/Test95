using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.MyAccount.Helpers;

internal sealed class AzurePdmDbHelper
{
    private const string AzurePdmConnectionStringSecretName = "FlownSQLDBConnString";
    private const string FreeProcessingStatus = "Free";
    private readonly string connectionString;

    public AzurePdmDbHelper()
    {
        connectionString = Vault.Default.GetSecretValue(AzurePdmConnectionStringSecretName);
        Assert.False(
            connectionString.IsNullOrWhiteSpace(),
            $"Expected Azure Key Vault secret '{AzurePdmConnectionStringSecretName}' to contain the PDM SQL connection string, but it was empty or unavailable.");
    }

    public static ClaimVsPointsFlownData CreateClaimData(string firstName, string lastName)
    {
        Assert.False(firstName.IsNullOrWhiteSpace(), "Expected a non-empty Flying Club first name for PDM claim data, but found an empty value.");
        Assert.False(lastName.IsNullOrWhiteSpace(), "Expected a non-empty Flying Club last name for PDM claim data, but found an empty value.");

        // The timestamp ticket number and varied in-window date prevent a prior claim from matching this flight transaction.
        string ticketNumber = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        DateTime departureDate = DateTime.UtcNow.Date.AddDays(-4 - RandomNumberGenerator.GetInt32(150));

        return new ClaimVsPointsFlownData(ticketNumber, firstName, lastName, departureDate);
    }

    public async Task CreateFlownDataAsync(ClaimVsPointsFlownData flownData)
    {
        const string query = """
            INSERT INTO [STG].[FLOWNDATA_FILE_DTL]
            (
                [FD_RECORD_TYPE], [FD_LOY_PRG_AIRLINE_CD], [FD_MEMBER_NUMBER], [FD_LAST_NAME], [FD_PNR_NAME],
                [FD_OPER_AIRLINE_CD], [FD_OPER_FLIGHT_NUM], [FD_MKT_AIRLINE_CD], [FD_MKT_FLIGHT_NUM],
                [FD_DEPARTURE_DT], [FD_ACTUAL_DEPARTUTE_DT], [FD_ORIGIN_PORT], [FD_DESTINATION_PORT],
                [FD_BKG_CLASS], [FD_FLOWN_CLASS], [FD_MKT_BKG_CLASS], [FD_FLOWN_CABIN_CLASS],
                [FD_REC_LOCATOR_OPER], [FD_PASSENGER_TYPE], [FD_COUPON_NUM], [FD_FARE_BASIS], [FD_POS],
                [FD_SEAT_NUM], [FD_TICKET_NUM], [FD_BKG_DT], [FD_AGENT_IATA_NUM], [FD_BKG_CHANNEL],
                [FD_CITY_PAIR], [SENT_TO_PARTNER], [FD_NO_SHOW], [PROCESSING_STATUS], [ACCEPT_REJECT_FLG],
                [RECORD_CREATE_DATE], [RECORD_UPDATE_DATE]
            )
            VALUES
            (
                @RecordType, NULL, NULL, @LastName, @FirstName,
                @OperatingAirlineCode, @OperatingFlightNumber, @MarketingAirlineCode, @MarketingFlightNumber,
                @DepartureDate, @DepartureDate, @OriginPort, @DestinationPort,
                @BookingClass, @FlownClass, @MarketingBookingClass, @FlownCabinClass,
                @RecordLocator, @PassengerType, @CouponNumber, @FareBasis, @PointOfSale,
                @SeatNumber, @TicketNumber, @BookingDate, @AgentIataNumber, @BookingChannel,
                @CityPair, @SentToPartner, @NoShow, @ProcessingStatus, @AcceptRejectFlag,
                SYSUTCDATETIME(), SYSUTCDATETIME()
            );
            """;

        await using SqlConnection connection = new(connectionString);
        await using SqlCommand command = new(query, connection);
        AddFlownDataParameters(command, flownData);

        await connection.OpenAsync();
        int insertedRows = await command.ExecuteNonQueryAsync();
        Assert.True(
            insertedRows == 1,
            $"Expected one PDM flown-data row to be inserted for ticket '{flownData.TicketNumber}', but SQL reported {insertedRows} inserted rows.");
    }

    public async Task<ClaimVsPointsFlownData?> RetrieveFlownDataAsync(string ticketNumber)
    {
        const string query = """
            SELECT TOP (1)
                [FD_TICKET_NUM], [FD_LAST_NAME], [FD_PNR_NAME], [FD_MEMBER_NUMBER], [PROCESSING_STATUS],
                [FD_DEPARTURE_DT], [FD_OPER_AIRLINE_CD], [FD_OPER_FLIGHT_NUM], [FD_ORIGIN_PORT], [FD_DESTINATION_PORT]
            FROM [STG].[FLOWNDATA_FILE_DTL]
            WHERE [FD_TICKET_NUM] = @TicketNumber
            ORDER BY [FD_ID] DESC;
            """;

        await using SqlConnection connection = new(connectionString);
        await using SqlCommand command = new(query, connection);
        command.Parameters.Add("@TicketNumber", SqlDbType.VarChar, 14).Value = ticketNumber;

        await connection.OpenAsync();
        await using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
            return null;

        return new ClaimVsPointsFlownData(
            GetString(reader, "FD_TICKET_NUM"),
            GetString(reader, "FD_PNR_NAME"),
            GetString(reader, "FD_LAST_NAME"),
            reader.GetDateTime(reader.GetOrdinal("FD_DEPARTURE_DT")))
        {
            MemberNumber = GetNullableString(reader, "FD_MEMBER_NUMBER"),
            ProcessingStatus = GetNullableString(reader, "PROCESSING_STATUS"),
            OperatingAirlineCode = GetString(reader, "FD_OPER_AIRLINE_CD"),
            OperatingFlightNumber = GetString(reader, "FD_OPER_FLIGHT_NUM"),
            OriginPort = GetString(reader, "FD_ORIGIN_PORT"),
            DestinationPort = GetString(reader, "FD_DESTINATION_PORT")
        };
    }

    public async Task DeleteFlownDataAsync(string ticketNumber)
    {
        Assert.False(ticketNumber.IsNullOrWhiteSpace(), "Expected a non-empty ticket number when removing synthetic PDM flown data.");

        const string query = """
            DELETE FROM [STG].[FLOWNDATA_FILE_DTL]
            WHERE [FD_TICKET_NUM] = @TicketNumber;
            """;

        await using SqlConnection connection = new(connectionString);
        await using SqlCommand command = new(query, connection);
        command.Parameters.Add("@TicketNumber", SqlDbType.VarChar, 14).Value = ticketNumber;

        await connection.OpenAsync();
        int deletedRows = await command.ExecuteNonQueryAsync();
        // The generated ticket uniquely identifies this test's staging record and prevents deleting shared PDM data.
        Assert.True(deletedRows == 1, $"Expected one synthetic PDM flown-data row to be removed for ticket '{ticketNumber}', but SQL reported {deletedRows} deleted rows.");
    }

    private static void AddFlownDataParameters(SqlCommand command, ClaimVsPointsFlownData flownData)
    {
        command.Parameters.Add("@RecordType", SqlDbType.VarChar, 2).Value = "02";
        command.Parameters.Add("@LastName", SqlDbType.VarChar, 30).Value = flownData.LastName;
        command.Parameters.Add("@FirstName", SqlDbType.VarChar, 50).Value = flownData.FirstName;
        command.Parameters.Add("@OperatingAirlineCode", SqlDbType.VarChar, 3).Value = flownData.OperatingAirlineCode;
        command.Parameters.Add("@OperatingFlightNumber", SqlDbType.VarChar, 4).Value = flownData.OperatingFlightNumber;
        command.Parameters.Add("@MarketingAirlineCode", SqlDbType.VarChar, 3).Value = flownData.OperatingAirlineCode;
        command.Parameters.Add("@MarketingFlightNumber", SqlDbType.VarChar, 4).Value = flownData.OperatingFlightNumber;
        command.Parameters.Add("@DepartureDate", SqlDbType.DateTime2).Value = flownData.DepartureDate;
        command.Parameters.Add("@OriginPort", SqlDbType.VarChar, 5).Value = flownData.OriginPort;
        command.Parameters.Add("@DestinationPort", SqlDbType.VarChar, 5).Value = flownData.DestinationPort;
        command.Parameters.Add("@BookingClass", SqlDbType.VarChar, 2).Value = "O";
        command.Parameters.Add("@FlownClass", SqlDbType.VarChar, 2).Value = "V";
        command.Parameters.Add("@MarketingBookingClass", SqlDbType.VarChar, 2).Value = "V";
        command.Parameters.Add("@FlownCabinClass", SqlDbType.VarChar, 2).Value = "Y";
        command.Parameters.Add("@RecordLocator", SqlDbType.VarChar, 32).Value = DataGenerator.RandomString(6, true).ToUpperInvariant();
        command.Parameters.Add("@PassengerType", SqlDbType.VarChar, 4).Value = "ADT";
        command.Parameters.Add("@CouponNumber", SqlDbType.Int).Value = 1;
        command.Parameters.Add("@FareBasis", SqlDbType.VarChar, 15).Value = "XY56DHSM";
        command.Parameters.Add("@PointOfSale", SqlDbType.VarChar, 9).Value = "Direct";
        command.Parameters.Add("@SeatNumber", SqlDbType.VarChar, 6).Value = "92C";
        command.Parameters.Add("@TicketNumber", SqlDbType.VarChar, 14).Value = flownData.TicketNumber;
        command.Parameters.Add("@BookingDate", SqlDbType.DateTime2).Value = flownData.DepartureDate.AddDays(-9);
        command.Parameters.Add("@AgentIataNumber", SqlDbType.VarChar, 32).Value = "7322343156";
        command.Parameters.Add("@BookingChannel", SqlDbType.VarChar, 14).Value = "RES00012";
        command.Parameters.Add("@CityPair", SqlDbType.VarChar, 10).Value = "LASLHR";
        command.Parameters.Add("@SentToPartner", SqlDbType.VarChar, 1).Value = "N";
        command.Parameters.Add("@NoShow", SqlDbType.VarChar, 1).Value = "N";
        command.Parameters.Add("@ProcessingStatus", SqlDbType.VarChar, 20).Value = FreeProcessingStatus;
        command.Parameters.Add("@AcceptRejectFlag", SqlDbType.VarChar, 1).Value = "A";
    }

    private static string GetString(SqlDataReader reader, string columnName) =>
        Convert.ToString(reader.GetValue(reader.GetOrdinal(columnName)), CultureInfo.InvariantCulture) ?? string.Empty;

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }
}

internal sealed record ClaimVsPointsFlownData(string TicketNumber, string FirstName, string LastName, DateTime DepartureDate)
{
    public string? MemberNumber { get; init; }
    public string? ProcessingStatus { get; init; }
    public string OperatingAirlineCode { get; init; } = "VS";
    public string OperatingFlightNumber { get; init; } = "157";
    public string OriginPort { get; init; } = "LAS";
    public string DestinationPort { get; init; } = "LHR";
}
