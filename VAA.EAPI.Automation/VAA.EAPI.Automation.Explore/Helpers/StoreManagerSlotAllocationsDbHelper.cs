using System.Data;
using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class StoreManagerSlotAllocationsDbHelper
    {
        private static readonly string connectionString = Vault.Default.GetSecretValue("Explore-HolidaysDbConnectionString");

        public static List<StoreManagerSlotAllocationDbRecord> GetSlotAllocations(string storeId, string slotId, DateTimeOffset startAtUtc)
        {
            List<StoreManagerSlotAllocationDbRecord> allocations = new();

            using SqlConnection connection = new(connectionString);

            using SqlCommand command = new("dbo.usp_StoreManager_GetSlotAllocations", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.CommandTimeout = 120;

            command.Parameters.Add("@StoreId", SqlDbType.NVarChar, 50).Value = storeId;

            command.Parameters.Add("@StoreSlotId", SqlDbType.Int).Value = Convert.ToInt32(slotId);

            command.Parameters.Add("@SlotDate", SqlDbType.Date).Value = startAtUtc.UtcDateTime.Date;

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                StoreManagerSlotAllocationDbRecord allocation = new()
                {
                    StoreSlotId = Convert.ToInt32(reader["StoreSlotId"]),

                    StoreId = reader["StoreId"]?.ToString() ?? string.Empty,

                    StartsAtUtc = (DateTimeOffset)reader["StartsAtUtc"],

                    EndsAtUtc = (DateTimeOffset)reader["EndsAtUtc"],

                    Capacity = Convert.ToInt32(reader["Capacity"]),

                    BookedCount = Convert.ToInt32(reader["BookedCount"]),

                    RemainingCapacity = Convert.ToInt32(reader["RemainingCapacity"]),

                    AllocationId = reader["AllocationId"]?.ToString() ?? string.Empty,

                    AllocationIndex = Convert.ToInt32(reader["AllocationIndex"]),

                    Status = reader["Status"]?.ToString() ?? string.Empty,

                    Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"]?.ToString(),

                    BookingId = reader["BookingId"] == DBNull.Value ? null : reader["BookingId"]?.ToString(),

                    CustomerRef = reader["CustomerRef"] == DBNull.Value ? null : reader["CustomerRef"]?.ToString()
                };

                allocations.Add(allocation);
            }

            return allocations;
        }
    }
}
