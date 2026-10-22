using System.Globalization;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.Microservice.CustomerTransaction;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    public class LoyaltyVoucherCreation : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly string _env;
        private readonly string _envUpper;
        private readonly string _customerCosmosDb;
        private readonly string _ocpSubKey;
        private readonly string _connectionString;

        private readonly FlyingClubMemberHelper _flyingClubMemberHelper;
        private readonly CosmosFixture _cosmosFixture;
        private readonly TokenFixture _tokenFixture;

        private const string LoyaltyAccountsTableName = "LoyaltyAccounts";
        private const string LoyaltyVouchersTableName = "LoyaltyVouchers";

        public LoyaltyVoucherCreation(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _cosmosFixture = cosmosFixture;
            _tokenFixture = tokenFixture;

            _env = Config.Environment;
            _envUpper = _env.ToUpperInvariant();
            _customerCosmosDb = _env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            _ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");
            _connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

            _flyingClubMemberHelper = new();

            _cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            _cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, _customerCosmosDb, Application.CustomerCosmosCollection);
            _cosmosFixture.RegisterType<CustomerSearch>(Application.CustomerCosmosDbEndpoint, _customerCosmosDb, Application.CustomerCosmosCollection);

            _tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");
        }

        [Theory]
        [Trait("Category", Traits.LoyaltyVoucherDataCreation)]
        [InlineData("Red", 1, 1, 1, 0, "")]
        [InlineData("Silver", 1, 1, 1, 0, "")]
        [InlineData("Gold", 1, 1, 1, 0, "")]
        [InlineData("Red", 3, 2, 2, 0, "")]
        [InlineData("Silver", 1, 2, 1, 0, "")]
        [InlineData("Gold", 1, 2, 1, 0, "")]
        [InlineData("Red", 3, 2, 1, 0, "")]
        // Supplies voucher selection tests with exactly two vouchers valid beyond the search window.
        [InlineData("Red", 1, 2, 0, 0, "")]
        // Deterministic two-voucher fixtures for the 3/4-passenger Companion acceptance cases.
        [InlineData("Red", 3, 2, 0, 0, "Flying Club reward voucher - Virgin Red US credit card")]
        [InlineData("Silver", 3, 3, 2, 0, "")]
        [InlineData("Gold", 3, 3, 1, 0, "")]
        // Supplies TC39 with more than two vouchers that remain valid beyond the normal search window.
        [InlineData("Red", 1, 3, 0, 0, "")]
        [InlineData("Red", 3, 1, 0, 300000, "")]
        [InlineData("Silver", 3, 1, 0, 300000, "")]
        [InlineData("Gold", 3, 1, 0, 300000, "")]
        public async Task AddLoyaltyVouchersToFlyingClubAccounts(
            string tierCode,
            int memberCount,
            int voucherCount,
            int vouchersExpiringWithinSixMonths,
            int minimumPoints,
            string voucherNameOverride)
        {
            TableStorageService<LoyaltyAccountsTableEntity> loyaltyAccountsStorageTableService = new()
            {
                TableName = LoyaltyAccountsTableName,
                ConnectionString = _connectionString
            };

            TableStorageService<LoyaltyVouchersTableEntity> loyaltyVouchersStorageTableService = new()
            {
                TableName = LoyaltyVouchersTableName,
                ConnectionString = _connectionString
            };

            // Index existing voucher keys for this environment so prior fixtures are not replaced.
            List<LoyaltyVouchersTableEntity> existingLoyaltyVouchers =
                (await loyaltyVouchersStorageTableService.RetrieveItemsAsync(default))
                    .Where(x => x.PartitionKey.Is(_env))
                    .ToList();

            HashSet<string> existingVoucherAccountRowKeys = new(
                existingLoyaltyVouchers.Select(voucher => voucher.RowKey),
                StringComparer.OrdinalIgnoreCase);

            List<LoyaltyAccountsTableEntity> loyaltyAccounts = (await loyaltyAccountsStorageTableService.RetrieveItemsAsync(default))
                .Where(x => !string.IsNullOrEmpty(x.RowKey) &&
                            x.PartitionKey.Is(_env) &&
                            x.Tier.Is(tierCode) &&
                            x.Points >= minimumPoints &&
                            !existingVoucherAccountRowKeys.Contains(x.RowKey))
                .ToList();

            Assert.True(loyaltyAccounts.Count > 0, $"Expected at least one LoyaltyAccounts entry for env '{_env}' and tier '{tierCode}', but found '{loyaltyAccounts.Count}'.");
            Assert.True(loyaltyAccounts.Count >= memberCount, $"Expected at least {memberCount} LoyaltyAccounts entries for env '{_env}' and tier '{tierCode}', but found {loyaltyAccounts.Count}.");
            Assert.InRange(vouchersExpiringWithinSixMonths, 0, voucherCount);
            Assert.True(minimumPoints >= 0, $"Expected non-negative minimum points, but found '{minimumPoints}'.");

            Random random = Random.Shared;

            for (int memberIndex = 0; memberIndex < memberCount; memberIndex++)
            {
                LoyaltyAccountsTableEntity account = loyaltyAccounts[random.Next(loyaltyAccounts.Count)];
                loyaltyAccounts.Remove(account);

                Assert.True(account is not null, $"Expected a LoyaltyAccounts storage-table entry for env '{_env}' and tier '{tierCode}', but found no entry.");

                string memberNumber = account.RowKey;
                string email = account.Email;

                Assert.False(email.IsNullOrEmpty(), $"For member {memberNumber}, LoyaltyAccounts entry has no email.");

                string queryForCustomerSearchDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

                QueryDefinition paramQuery = new QueryDefinition(queryForCustomerSearchDoc)
                    .WithParameter("@docType", Application.DocTypeCustomerSearch)
                    .WithParameter("@partitionKey", memberNumber);

                CustomerSearch? customerSearchDoc = _cosmosFixture.CosmosQueryItem<CustomerSearch>(paramQuery);

                Assert.True(customerSearchDoc is not null, $"Member '{memberNumber}': expected a CustomerSearch document from Cosmos DB, but found no document.");

                string customerId = customerSearchDoc.CustomerId;

                Assert.False(customerId.IsNullOrEmpty(), $"For member {memberNumber}, CustomerSearch doc has no CustomerId.");

                RetrieveVoucherResponse[] availableVouchers = Array.Empty<RetrieveVoucherResponse>();

                for (int voucherIndex = 0; voucherIndex < voucherCount; voucherIndex++)
                {
                    string? selectedVoucherName = !voucherNameOverride.IsNullOrEmpty()
                        ? voucherNameOverride
                        : tierCode switch
                    {
                        "Red" or "Silver" => Application.VoucherTypes[random.Next(0, 3)],
                        "Gold" => Application.VoucherTypes[random.Next(3, Application.VoucherTypes.Length)],
                        _ => null
                    };
                    Assert.True(selectedVoucherName is not null, $"Expected tier code 'Red', 'Silver', or 'Gold' when creating vouchers, but found '{tierCode}'.");
                    string voucherName = selectedVoucherName ?? string.Empty;

                    // Create the requested near-expiry vouchers before adding future-expiry vouchers.
                    bool expiresWithinSixMonths = voucherIndex < vouchersExpiringWithinSixMonths;
                    int? expiryMonthsFromNow = expiresWithinSixMonths
                        ? voucherIndex + 2
                        : 18 + voucherIndex;
                    DateTime today = DateTime.UtcNow.Date;
                    DateTime sixMonthCutoff = today.AddMonths(6);

                    bool VoucherMatchesExpectedAward(RetrieveVoucherResponse voucher) =>
                        voucher.VoucherName.Is(voucherName) &&
                        DateTime.TryParse(
                            voucher.ExpiryDate,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal,
                            out DateTime expiryDate) &&
                        (expiresWithinSixMonths
                            ? expiryDate.Date >= today && expiryDate.Date <= sixMonthCutoff
                            : expiryDate.Date > sixMonthCutoff);

                    if (voucherIndex == 0)
                    {
                        using HttpResponseMessage existingVoucherResponse = Microservice
                            .RetrieveVouchers(_tokenFixture.GetToken("customertxn"), _ocpSubKey, customerId)
                            .EnsureIsSuccessStatusCode();
                        string existingVoucherContent = await existingVoucherResponse.Content.ReadAsStringAsync();
                        RetrieveVoucherResponse[] existingVouchers = JsonConvert.DeserializeObject<RetrieveVoucherResponse[]>(existingVoucherContent) ?? Array.Empty<RetrieveVoucherResponse>();
                        availableVouchers = existingVouchers.Where(voucher => voucher.Status.Is("Available")).ToArray();
                    }

                    int matchingVoucherCountBeforeAward = availableVouchers.Count(VoucherMatchesExpectedAward);

                    await _flyingClubMemberHelper.AwardVoucherToFlyingClubAccount(
                        customerId,
                        memberNumber,
                        voucherName,
                        expiresWithinSixMonths,
                        expiryMonthsFromNow);

                    await RetryHelper.RunWithRetryAsync(async () =>
                    {
                        using HttpResponseMessage retrieveVoucher = Microservice
                            .RetrieveVouchers(_tokenFixture.GetToken("customertxn"), _ocpSubKey, customerId)
                            .EnsureIsSuccessStatusCode();

                        string content = await retrieveVoucher.Content.ReadAsStringAsync();

                        RetrieveVoucherResponse[] vouchers = JsonConvert.DeserializeObject<RetrieveVoucherResponse[]>(content) ?? Array.Empty<RetrieveVoucherResponse>();

                        availableVouchers = vouchers.Where(voucher => voucher.Status.Is("Available")).ToArray();

                        int matchingVoucherCountAfterAward = availableVouchers.Count(VoucherMatchesExpectedAward);

                        Assert.True(
                            matchingVoucherCountAfterAward > matchingVoucherCountBeforeAward,
                            $"Member '{memberNumber}': expected voucher '{voucherName}' to increase the matching available count from {matchingVoucherCountBeforeAward}, but found {matchingVoucherCountAfterAward} after the retry window.");
                    }, maxRetries: 5, delay: 1000);
                }

                await AddVouchersToLoyaltyVouchersStorageTable(loyaltyVouchersStorageTableService, account, availableVouchers, tierCode);
            }
        }

        private async Task AddVouchersToLoyaltyVouchersStorageTable(
            TableStorageService<LoyaltyVouchersTableEntity> tableStorageService,
            LoyaltyAccountsTableEntity account,
            RetrieveVoucherResponse[] vouchers,
            string tierCode)
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime sixMonthCutoff = today.AddMonths(6);

            // Persist whether any available voucher expires in the next six months.
            bool hasVoucherExpiringWithinSixMonths = vouchers.Any(voucher =>
                DateTime.TryParse(
                    voucher.ExpiryDate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out DateTime expiryDate) &&
                expiryDate.Date >= today &&
                expiryDate.Date <= sixMonthCutoff);

            await tableStorageService.InsertOrMergeAsync(new LoyaltyVouchersTableEntity
            {
                PartitionKey = _envUpper,
                RowKey = account.RowKey,
                Vouchers = JsonConvert.SerializeObject(vouchers),
                TierCode = tierCode,
                Email = account.Email,
                FirstName = account.FirstName,
                LastName = account.LastName,
                VoucherCreatedDate = DateTime.UtcNow,
                Points = account.Points,
                IsVoucherExpiryWithInSixMonths = hasVoucherExpiringWithinSixMonths
            });
        }
    }
}
