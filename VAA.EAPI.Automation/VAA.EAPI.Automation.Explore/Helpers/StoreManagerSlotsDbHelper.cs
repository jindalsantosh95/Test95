using System.Data;
using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class StoreManagerSlotsDbHelper
    {
        private static readonly string connectionString = Vault.Default.GetSecretValue("Explore-HolidaysDbConnectionString");

        public static List<StoreManagerSlotDbRecord> GetSlots(string storeId, DateTimeOffset fromUtc, DateTimeOffset toUtc)
        {
            List<StoreManagerSlotDbRecord> slots = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("dbo.usp_StoreManager_GetSlots", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.CommandTimeout = 120;

            command.Parameters.Add("@StoreId", SqlDbType.NVarChar, 50).Value = storeId;

            command.Parameters.Add("@FromUtc", SqlDbType.DateTimeOffset).Value = fromUtc;

            command.Parameters.Add("@ToUtc", SqlDbType.DateTimeOffset).Value = toUtc;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                StoreManagerSlotDbRecord slot = new()
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

                slots.Add(slot);
            }

            return slots;
        }
    }
}
