using System.Globalization;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.Microservice.CustomerTransaction;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    [Collection("Updates")]
    public class LoyaltyVoucherUpdates : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        public const string LoyaltyVoucherArchiveStorageTableName = "LoyaltyVouchersArchieve";
        private const string LoyaltyVouchersTableName = "LoyaltyVouchers";

        private readonly string _env;
        private readonly string _envUpper;
        private readonly string _customerCosmosDb;
        private readonly string _ocpSubKey;
        private readonly string _connectionString;

        private readonly CosmosFixture _cosmosFixture;
        private readonly TokenFixture _tokenFixture;

        public LoyaltyVoucherUpdates(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _cosmosFixture = cosmosFixture;
            _tokenFixture = tokenFixture;

            _env = Config.Environment;
            _envUpper = _env.ToUpperInvariant();
            _customerCosmosDb = _env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            _ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");
            _connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

            _cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            _cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, _customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<CustomerSearch>(Application.CustomerCosmosDbEndpoint, _customerCosmosDb, Application.CustomerCosmosCollection);

            _tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");
        }

        [Fact]
        [Trait("Category", Traits.LoyaltyVoucherDataUpdate)]
        public async Task UpdateLoyaltyVoucherTableData()
        {
            TableStorageService<LoyaltyVouchersTableEntity> loyaltyVouchersStorageTableService = new()
            {
                TableName = LoyaltyVouchersTableName,
                ConnectionString = _connectionString
            };

            List<LoyaltyVouchersTableEntity> results = await loyaltyVouchersStorageTableService.RetrieveItemsAsync(default);

            results = results.Where(x => x.PartitionKey.Is(_envUpper)).ToList();

            foreach (LoyaltyVouchersTableEntity record in results)
            {
                if (string.IsNullOrEmpty(record.RowKey))
                {
                    continue;
                }

                string queryForCustomerSearchDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

                QueryDefinition paramQuery = new QueryDefinition(queryForCustomerSearchDoc)
                    .WithParameter("@docType", Application.DocTypeCustomerSearch)
                    .WithParameter("@partitionKey", record.RowKey);

                CustomerSearch? customerSearchDoc = _cosmosFixture.CosmosQueryItem<CustomerSearch>(paramQuery);

                Assert.True(customerSearchDoc is not null, $"Member '{record.RowKey}': expected a CustomerSearch document from Cosmos DB, but found no document.");

                string customerId = customerSearchDoc.CustomerId;

                using HttpResponseMessage retrieveVoucher = Microservice
                    .RetrieveVouchers(_tokenFixture.GetToken("customertxn"), _ocpSubKey, customerId)
                    .EnsureIsSuccessStatusCode();

                string content = await retrieveVoucher.Content.ReadAsStringAsync();

                RetrieveVoucherResponse[] vouchers = JsonConvert.DeserializeObject<RetrieveVoucherResponse[]>(content) ?? Array.Empty<RetrieveVoucherResponse>();

                RetrieveVoucherResponse[] availableVouchers = vouchers.Where(x => x.Status.Is("Available")).ToArray();

                if (availableVouchers.Length <= 0)
                {
                    await loyaltyVouchersStorageTableService.DeleteAsync(new LoyaltyVouchersTableEntity()
                    {
                        PartitionKey = record.PartitionKey,
                        RowKey = record.RowKey,
                    });

                    continue;
                }

                DateTime today = DateTime.UtcNow.Date;
                DateTime sixMonthCutoff = today.AddMonths(6);
                bool hasVoucherExpiringWithinSixMonths = availableVouchers.Any(voucher =>
                    DateTime.TryParse(
                        voucher.ExpiryDate,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal,
                        out DateTime expiryDate) &&
                    expiryDate.Date >= today &&
                    expiryDate.Date <= sixMonthCutoff);

                await loyaltyVouchersStorageTableService.InsertOrMergeAsync(new LoyaltyVouchersTableEntity()
                {
                    PartitionKey = record.PartitionKey,
                    RowKey = record.RowKey,
                    Vouchers = JsonConvert.SerializeObject(availableVouchers),
                    TierCode = record.TierCode,
                    Email = record.Email,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    VoucherCreatedDate = record.VoucherCreatedDate,
                    Points = record.Points,
                    IsVoucherExpiryWithInSixMonths = hasVoucherExpiringWithinSixMonths
                });
            }
        }
    }
}
