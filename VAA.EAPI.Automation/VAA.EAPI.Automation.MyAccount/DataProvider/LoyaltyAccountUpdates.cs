using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    [Collection("Updates")]
    public class LoyaltyAccountUpdates : IClassFixture<TokenFixture>
    {
        private const string LoyaltyAccountsTableName = "LoyaltyAccounts";
        private const string InvalidCredentialsErrorMessage = "Account member SignIn failed due to invalid username or password";

        private readonly FlyingClubMemberHelper flyingClubMemberHelper = new();
        private readonly string environment = Config.Environment;
        private readonly TokenFixture tokenFixture;
        private static readonly string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        public LoyaltyAccountUpdates(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");
        }

        [Fact]
        [Trait("Category", Traits.LoyaltyAccountStoragePointsUpdate)]
        public async Task UpdateAllLoyaltyAccountPointsToStorageTable()
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = LoyaltyAccountsTableName;
            tableStorageService.ConnectionString = connectionString;

            List<LoyaltyAccountsTableEntity> records = (await tableStorageService.RetrieveItemsAsync(default))
                .Where(x => !string.IsNullOrEmpty(x.RowKey) && x.PartitionKey.Is(environment) && !x.Tier.Is("VAC"))
                .ToList();

            Assert.NotEmpty(records);

            foreach (LoyaltyAccountsTableEntity record in records)
            {
                string fcMembershipNumber = record.RowKey;

                HttpResponseMessage vRedResponseAPI = Microservice.Vred(tokenFixture.GetToken("VRedMS"), fcMembershipNumber).EnsureIsSuccessStatusCode();

                string content = await vRedResponseAPI.Content.ReadAsStringAsync();

                JObject result = JObject.Parse(content);

                int vredPoints = result.RequireToken("balance.usable").Value<int>();

                if (record.Points != vredPoints)
                {
                    await UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, vredPoints);
                }
            }
        }

        [Theory]
        [InlineData(1000000)]
        [Trait("Category", Traits.LoyaltyAccountStoragePointsUpdate)]
        public async Task AddPointsToAllLoyaltyAccountStorageTableFCAccounts(int points)
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = LoyaltyAccountsTableName;
            tableStorageService.ConnectionString = connectionString;

            List<LoyaltyAccountsTableEntity> records = (await tableStorageService.RetrieveItemsAsync(default))
                .Where(x => !string.IsNullOrEmpty(x.RowKey) && x.PartitionKey.Is(environment) && !x.Tier.Is("VAC"))
                .ToList();

            Assert.NotEmpty(records);

            foreach (LoyaltyAccountsTableEntity record in records)
            {
                string fcMembershipNumber = record.RowKey;
                int storagePoints = record.Points;
                bool isTransactionCreated = await flyingClubMemberHelper.AddLoyaltyPointsToTheFlyingClubAccount(fcMembershipNumber, points);

                Assert.True(isTransactionCreated, $"Member '{fcMembershipNumber}': expected loyalty-points credit transaction creation to succeed, but found success '{isTransactionCreated}'.");

                long expectedPointBalance = (long)storagePoints + points;
                long memberPointBalance = 0;

                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    // VRed point credits are eventually consistent, so retry only the observable balance condition.
                    memberPointBalance = await flyingClubMemberHelper.GetPointBalanceOfFlyingClubAccount(fcMembershipNumber);
                    Assert.Equal(expectedPointBalance, memberPointBalance);
                }, 10, 2000);

                // Add the confirmed point balance to the source storage record.
                await UpdateLoyaltyAccountToStorageTable(fcMembershipNumber, (int)expectedPointBalance);
            }
        }

        [Fact]
        [Trait("Category", Traits.LoyaltyAccountStoragePointsUpdate)]
        public async Task UpdateLoyaltyAccountsTable()
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = LoyaltyAccountsTableName;
            tableStorageService.ConnectionString = connectionString;

            List<LoyaltyAccountsTableEntity> records = (await tableStorageService.RetrieveItemsAsync(default))
                .Where(x => !string.IsNullOrEmpty(x.RowKey) && x.PartitionKey.Is(environment))
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            Assert.NotEmpty(records);

            foreach (LoyaltyAccountsTableEntity record in records)
            {
                string memberNumber = record.RowKey;

                string email = record.Email;

                HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(environment, email, memberNumber, Application.LoginPassword).EnsureIsSuccessStatusCode();

                JObject graphQlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());
                JToken? graphQlErrors = graphQlResponse["errors"];

                // A confirmed invalid-credential GraphQL error identifies a stale storage record that must be removed.
                bool hasInvalidCredentialsError = graphQlErrors?.Children()
                    .Select(error => error["extensions"]?["message"]?.ToString())
                    .Any(message => message?.Contains(InvalidCredentialsErrorMessage, StringComparison.OrdinalIgnoreCase) == true) == true;

                if (hasInvalidCredentialsError)
                {
                    await LoyaltyAccountDeletion.DeleteLoyaltyAccountsFromStorageTable(LoyaltyAccountsTableName, memberNumber);
                    continue;
                }

                Assert.True(
                    graphQlErrors is null || !graphQlErrors.HasValues,
                    $"Member '{memberNumber}': expected no GraphQL sign-in errors, but found '{graphQlErrors}'.");

                string? idToken = graphQlResponse.SelectToken("data.accountMemberSignIn.idToken")?.ToString();
                string? continuationToken = graphQlResponse.SelectToken("data.accountMemberSignIn.continuationToken")?.ToString();

                Assert.True(
                    !idToken.IsNullOrEmpty() || !continuationToken.IsNullOrWhiteSpace(),
                    $"Member '{memberNumber}': expected an ID token or MFA continuation token, but found ID token '{(idToken.IsNullOrEmpty() ? "<empty>" : "<populated>")}' and continuation token '{(continuationToken.IsNullOrWhiteSpace() ? "<empty>" : "<populated>")}'.");
            }
        }

        public static async Task UpdateLoyaltyAccountToStorageTable(string loyaltyAccountNumber, int points)
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = LoyaltyAccountsTableName;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new LoyaltyAccountsTableEntity()
            {
                PartitionKey = Config.Environment.ToUpperInvariant(),
                RowKey = loyaltyAccountNumber,
                Points = points
            });
        }

        internal static async Task ArchiveAllLoyaltyAccountInStorageTable(string table, LoyaltyAccountsTableEntity record)
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = table;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new LoyaltyAccountsTableEntity()
            {
                PartitionKey = Config.Environment.ToUpperInvariant(),
                RowKey = record.RowKey,
                Tier = record.Tier,
                Points = record.Points,
                FirstName = record.FirstName,
                LastName = record.LastName,
                Email = record.Email,
                DateOfBirth = record.DateOfBirth,
                UnderSixteen = record.UnderSixteen,
                CoBrand = record.CoBrand,
                CountryAddress = record.CountryAddress,
                IsWingsMember = record.IsWingsMember,
                ForHouseHold = record.ForHouseHold,
                CreatedDate = record.CreatedDate
            });
        }

    }
}
