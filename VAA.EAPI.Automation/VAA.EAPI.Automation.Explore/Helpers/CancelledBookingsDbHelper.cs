using System.Data;
using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class CancelledBookingsDbHelper
    {
        private static readonly string connectionString = Vault.Default.GetSecretValue("Explore-HolidaysDbConnectionString");

        public static List<CancelledBookingDbRecord> GetBookings(string storeId, DateTime date, string status)
        {
            List<CancelledBookingDbRecord> bookings = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("dbo.usp_StoreManager_GetBookings", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.CommandTimeout = 120;

            command.Parameters.Add("@StoreId", SqlDbType.NVarChar, 20).Value = storeId;

            command.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;

            command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status;

            command.Parameters.Add("@Query", SqlDbType.NVarChar, 100).Value = DBNull.Value;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                CancelledBookingDbRecord booking = new()
                {
                    BookingId = reader["BookingId"]?.ToString() ?? string.Empty,

                    StoreId = reader["StoreId"]?.ToString() ?? string.Empty,

                    StoreSlotId = reader["StoreSlotId"] == DBNull.Value ? null : Convert.ToInt32(reader["StoreSlotId"]),

                    CustomerRef = reader["CustomerRef"]?.ToString() ?? string.Empty,

                    Status = reader["Status"]?.ToString() ?? string.Empty,

                    StartsAtUtc = (DateTimeOffset)reader["StartsAtUtc"],

                    EndsAtUtc = (DateTimeOffset)reader["EndsAtUtc"],

                    Capacity = Convert.ToInt32(reader["Capacity"]),

                    CreatedUtc = (DateTimeOffset)reader["CreatedUtc"],

                    UpdatedUtc = (DateTimeOffset)reader["UpdatedUtc"],

                    Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"]?.ToString()
                };

                bookings.Add(booking);
            }

            return bookings;
        }
    }
}
