using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    [Collection("Deletion")]
    public class LoyaltyVoucherDeletion
    {

        private static readonly string ConnectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        public const string LoyaltyVoucherStorageTableName = "LoyaltyVouchers";
        public const string LoyaltyVoucherArchiveStorageTableName = "LoyaltyVouchersArchieve";

        [Fact]
        [Trait("Category", Traits.LoyaltyVoucherStorageDataDelete)]
        public async Task DeleteAllLoyaltyVoucherAccountsFromStorageTable()
        {
            string envPartitionKey = Config.Environment.ToUpperInvariant();

            TableStorageService<LoyaltyVouchersTableEntity> tableStorageService = new();
            tableStorageService.TableName = LoyaltyVoucherStorageTableName;
            tableStorageService.ConnectionString = ConnectionString;

            List<LoyaltyVouchersTableEntity> results = await tableStorageService.RetrieveItemsAsync(default);

            foreach (LoyaltyVouchersTableEntity record in results.Where(record => record.PartitionKey.Is(envPartitionKey)))
            {
                if (string.IsNullOrEmpty(record.RowKey))
                {
                    continue;
                }

                await ArchiveAllLoyaltyVoucherAccountsInStorageTable(record);

                await tableStorageService.DeleteAsync(new LoyaltyVouchersTableEntity()
                {
                    PartitionKey = record.PartitionKey,
                    RowKey = record.RowKey,
                });
            }
        }

        private static async Task ArchiveAllLoyaltyVoucherAccountsInStorageTable(LoyaltyVouchersTableEntity record)
        {
            TableStorageService<LoyaltyVouchersTableEntity> tableStorageService = new();
            tableStorageService.TableName = LoyaltyVoucherArchiveStorageTableName;
            tableStorageService.ConnectionString = ConnectionString;

            await tableStorageService.InsertOrMergeAsync(new LoyaltyVouchersTableEntity()
            {
                PartitionKey = record.PartitionKey,
                RowKey = record.RowKey,
                Vouchers = record.Vouchers,
                TierCode = record.TierCode,
                Email = record.Email,
                FirstName = record.FirstName,
                LastName = record.LastName,
                VoucherCreatedDate = record.VoucherCreatedDate,
                Points = record.Points,
                IsVoucherExpiryWithInSixMonths = record.IsVoucherExpiryWithInSixMonths
            });
        }

    }
}
