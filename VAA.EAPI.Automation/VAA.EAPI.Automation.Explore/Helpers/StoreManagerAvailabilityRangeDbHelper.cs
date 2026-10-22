using System.Data;
using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class StoreManagerAvailabilityRangeDbHelper
    {
        private static readonly string connectionString = Vault.Default.GetSecretValue("Explore-HolidaysDbConnectionString");

        public static List<StoreManagerAvailabilityRangeDbRecord> GetAvailabilityRange(string storeId, DateTime fromDate, DateTime toDate)
        {
            List<StoreManagerAvailabilityRangeDbRecord> availability = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("dbo.usp_StoreManager_GetAvailability", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.CommandTimeout = 120;

            command.Parameters.Add("@StoreId", SqlDbType.NVarChar, 50).Value = storeId;

            command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;

            command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                StoreManagerAvailabilityRangeDbRecord window = new()
                {
                    StoreSlotId = Convert.ToInt32(reader["StoreSlotId"]),

                    StoreId = reader["StoreId"]?.ToString() ?? string.Empty,

                    StartsAtUtc = (DateTimeOffset)reader["StartsAtUtc"],

                    EndsAtUtc = (DateTimeOffset)reader["EndsAtUtc"],

                    Capacity = Convert.ToInt32(reader["Capacity"]),

                    RemainingCapacity = Convert.ToInt32(reader["RemainingCapacity"]),

                    Status = reader["Status"]?.ToString() ?? string.Empty,

                    Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"]?.ToString()
                };

                availability.Add(window);
            }

            return availability;
        }
    }
}
