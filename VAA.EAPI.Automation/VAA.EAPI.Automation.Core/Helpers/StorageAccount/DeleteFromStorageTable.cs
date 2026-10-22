using Azure;
using Azure.Data.Tables;

namespace VAA.EAPI.Automation.Core.Helpers.StorageAccount
{
    public class DeleteFromStorageTable
    {
        private static readonly string _env = Config.Environment;
        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        public static async Task DeleteFromStorage(string tableName, DateTime currentWeekMonday)
        {
            TableClient tableClient = new(connectionString, tableName);

            try
            {
                // Query for records before current week's Monday
                AsyncPageable<TableEntity> query = tableClient.QueryAsync<TableEntity>(filter:
                    $"Timestamp lt datetime'{currentWeekMonday:yyyy-MM-dd}T00:00:00.0000000Z'");

                await foreach (TableEntity entity in query)
                {
                    try
                    {
                        await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, entity.ETag);
                    }
                    catch (RequestFailedException ex)
                    {
                        Console.WriteLine($"Error deleting entity: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in delete operation: {ex.Message}");
                throw;
            }
        }

        public static async Task DeleteLoyaltyAccountRecordByMemberNumber(string memberNumber)
        {
            if (string.IsNullOrWhiteSpace(memberNumber))
            {
                Console.WriteLine("Skipping LoyaltyAccounts delete cleanup because member number is empty.");
                return;
            }

            TableClient tableClient = new(connectionString, "LoyaltyAccounts");

            try
            {
                await tableClient.DeleteEntityAsync(_env, memberNumber, ETag.All);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"No LoyaltyAccounts record found for member number '{memberNumber}' in env partition '{_env}'.");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error deleting LoyaltyAccounts record (partitionKey={_env}, rowKey={memberNumber}): {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error deleting LoyaltyAccounts record (partitionKey={_env}, rowKey={memberNumber}): {ex.Message}");
            }
        }

    }
}
