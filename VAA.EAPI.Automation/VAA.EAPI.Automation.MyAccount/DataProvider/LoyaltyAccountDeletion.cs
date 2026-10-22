using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    [Collection("Deletion")]
    public class LoyaltyAccountDeletion
    {
        private const string LoyaltyAccountsTableName = "LoyaltyAccounts";

        private const string LoyaltyAccountsArchieveTableName = "LoyaltyAccountsArchieve";

        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        [Fact]
        [Trait("Category", Traits.LoyaltyAccountStorageDataDelete)]
        public async Task DeleteAllLoyaltyAccountInStorageTable()
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = LoyaltyAccountsTableName;
            tableStorageService.ConnectionString = connectionString;

            System.Collections.Generic.IEnumerable<LoyaltyAccountsTableEntity> results = (await tableStorageService.RetrieveItemsAsync(default)).Where(x => !string.IsNullOrEmpty(x.RowKey) && x.PartitionKey.Is(Config.Environment));

            foreach (LoyaltyAccountsTableEntity record in results)
            {
                await LoyaltyAccountUpdates.ArchiveAllLoyaltyAccountInStorageTable(LoyaltyAccountsArchieveTableName, record);

                await DeleteLoyaltyAccountsFromStorageTable(tableStorageService.TableName, record.RowKey);
            }
        }

        internal static async Task DeleteLoyaltyAccountsFromStorageTable(string table, string loyaltyAccountNumber)
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = table;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.DeleteAsync(new LoyaltyAccountsTableEntity()
            {
                PartitionKey = Config.Environment.ToUpper(),
                RowKey = loyaltyAccountNumber,
            });
        }

    }
}
