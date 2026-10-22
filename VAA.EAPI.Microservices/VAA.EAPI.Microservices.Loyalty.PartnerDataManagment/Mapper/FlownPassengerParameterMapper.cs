using System.Data;
using System.Globalization;
using Dapper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Mapper
{
    public static class FlownPassengerParameterMapper
    {
        public static DynamicParameters Create(FlownPassengerRequest flownDetails)
        {
            ArgumentNullException.ThrowIfNull(flownDetails);
            if (flownDetails.EventFlight is null) throw new ArgumentException("Event flight is required.", nameof(flownDetails));

            var parameters = new DynamicParameters();
            var passenger = flownDetails.Passengers?.FirstOrDefault();
            var seat = flownDetails.Seats?.FirstOrDefault();
            var eventFlight = flownDetails.EventFlight;
            var now = DateTime.UtcNow;
            var scheduledDepartureDate = Required(NullIfDefault(eventFlight.DepartureDate), "FD_DEPARTURE_DT");
            var lastName = Required(passenger?.LastName, "FD_LAST_NAME");
            var pnrName = Required(FirstNonEmpty(ExtractPnrName(flownDetails.PnrName), passenger?.FirstName), "FD_PNR_NAME");
            var operatingFlightNumber = Required(FirstNonEmpty(eventFlight.OperatingFlightNumber, eventFlight.FlightNumber), "FD_OPER_FLIGHT_NUM");
            var originPort = Required(eventFlight.DepartureAirport, "FD_ORIGIN_PORT");
            var destinationPort = Required(eventFlight.ArrivalAirport, "FD_DESTINATION_PORT");
            var ticketNumber = Required(flownDetails.TicketNumber, "FD_TICKET_NUM");

            parameters.Add("@FD_RECORD_TYPE", "02", DbType.String);
            parameters.Add("@FD_LOY_PRG_AIRLINE_CD", NullIfWhiteSpace(flownDetails.LoyaltyAirlineCode), DbType.String);
            parameters.Add("@FD_MEMBER_NUMBER", NullIfWhiteSpace(flownDetails.LoyaltyAccountNumber), DbType.String);
            parameters.Add("@FD_LAST_NAME", lastName, DbType.String);
            parameters.Add("@FD_PNR_NAME", pnrName, DbType.String);
            parameters.Add("@FD_OPER_AIRLINE_CD", FirstNonEmpty(eventFlight.OperatingCarrier, eventFlight.FlightCarrier), DbType.String);
            parameters.Add("@FD_OPER_FLIGHT_NUM", operatingFlightNumber, DbType.String);
            parameters.Add("@FD_MKT_AIRLINE_CD", FirstNonEmpty(eventFlight.MarketingCarrier, eventFlight.FlightCarrier), DbType.String);
            parameters.Add("@FD_MKT_FLIGHT_NUM", FirstNonEmpty(eventFlight.MarketingFlightNumber, eventFlight.FlightNumber), DbType.String);
            parameters.Add("@FD_DEPARTURE_DT", scheduledDepartureDate, DbType.DateTime);
            parameters.Add("@FD_ACTUAL_DEPARTUTE_DT", scheduledDepartureDate, DbType.DateTime);
            parameters.Add("@FD_ORIGIN_PORT", originPort, DbType.String);
            parameters.Add("@FD_DESTINATION_PORT", destinationPort, DbType.String);
            parameters.Add("@FD_BKG_CLASS", eventFlight.BookingClass, DbType.String);
            parameters.Add("@FD_FLOWN_CLASS", eventFlight.BookingClass, DbType.String);
            parameters.Add("@FD_MKT_BKG_CLASS", eventFlight.BookingClass, DbType.String);
            parameters.Add("@FD_FLOWN_CABIN_CLASS", seat?.CompartmentNumber, DbType.String);
            parameters.Add("@FD_REC_LOCATOR_OPER", flownDetails.BookingReference?.PnrLocator, DbType.String);
            parameters.Add("@FD_PASSENGER_TYPE", MapPassengerType(passenger?.PassengerType), DbType.String);
            parameters.Add("@FD_FARE_BASIS", NullIfWhiteSpace(flownDetails.FareBasisCode), DbType.String);
            parameters.Add("@FD_POS", MapPointOfSale(flownDetails.PointOfSale), DbType.String);
            parameters.Add("@FD_SEAT_NUM", seat?.CurrentSeatAssignment, DbType.String);
            parameters.Add("@FD_TICKET_NUM", ticketNumber, DbType.String);
            parameters.Add("@FD_TICKET_DESIGNATOR", NullIfWhiteSpace(flownDetails.TicketDesignator), DbType.String);
            parameters.Add("@FD_COUPON_NUM", flownDetails.CouponNumber, DbType.String);
            parameters.Add("@FD_BKG_DT", ParseOrderDate(flownDetails.TicketIssueDate), DbType.DateTime);
            parameters.Add("@FD_AGENT_IATA_NUM", flownDetails.AgentIataNumber, DbType.String);
            parameters.Add("@FD_BKG_CHANNEL", flownDetails.TicketOriginalCity, DbType.String);
            parameters.Add("@FD_CITY_PAIR", CreateCityPair(originPort, destinationPort), DbType.String);
            parameters.Add("@FD_OPER_AIR_AUTH_NUM", ExtractDigits(flownDetails.OperatingAirAuthNumber), DbType.String);
            parameters.Add("@SENT_TO_PARTNER", "N", DbType.String);
            parameters.Add("@FD_NO_SHOW", "N", DbType.String);
            parameters.Add("@RECORD_ID", ParseNullableInt(flownDetails.OrderId), DbType.Int32);
            parameters.Add("@PROCESSING_STATUS", "Free", DbType.String);
            parameters.Add("@ACCEPT_REJECT_FLG", "A", DbType.String);
            parameters.Add("@RECORD_CREATE_DATE", now, DbType.DateTime);
            parameters.Add("@UPR_RNG_CHK_FLG", "N", DbType.String);

            return parameters;
        }

        private static int? ParseNullableInt(string? value) =>
            int.TryParse(value, out var parsedValue) ? parsedValue : null;

        private static DateTime? ParseOrderDate(string? value) =>
            DateTime.TryParseExact(value, "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedValue)
                ? parsedValue
                : null;

        private static string? ExtractPnrName(string? passengerName)
        {
            if (string.IsNullOrWhiteSpace(passengerName)) return null;

            var separatorIndex = passengerName.LastIndexOf('/');
            return separatorIndex >= 0 && separatorIndex < passengerName.Length - 1
                ? passengerName[(separatorIndex + 1)..]
                : passengerName;
        }

        private static string? MapPointOfSale(string? sourceValue) =>
            sourceValue?.Trim().ToUpperInvariant() switch
            {
                "AGCY" or "AGENCY" => "Indirect",
                _ => sourceValue
            };

        private static string MapPassengerType(string? sourceValue)
        {
            var passengerType = sourceValue?.Trim();

            return string.IsNullOrWhiteSpace(passengerType) || passengerType.Length == 1
                ? "ADT"
                : passengerType;
        }

        private static string? NullIfWhiteSpace(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private static string Required(string? value, string columnName) =>
            NullIfWhiteSpace(value) ?? throw new MissingRequiredFlownPassengerFieldException(columnName);

        private static DateTime Required(DateTime? value, string columnName) =>
            value ?? throw new MissingRequiredFlownPassengerFieldException(columnName);

        private static string? ExtractDigits(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var digits = new string(value.Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digits) ? null : digits;
        }

        private static string? FirstNonEmpty(string? primaryValue, string? fallbackValue) =>
            !string.IsNullOrWhiteSpace(primaryValue) ? primaryValue : fallbackValue;

        private static string? CreateCityPair(string? departureAirport, string? arrivalAirport) =>
            string.IsNullOrWhiteSpace(departureAirport) || string.IsNullOrWhiteSpace(arrivalAirport)
                ? null
                : $"{departureAirport}{arrivalAirport}";

        private static DateTime? NullIfDefault(DateTime? value) =>
            value is null || value == DateTime.MinValue ? null : value;
    }

}
