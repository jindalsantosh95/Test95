using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;

namespace VAA.EAPI.Automation.MyAccount.Fixtures
{
    public sealed class StoredLoyaltyMemberAccountFixture : IAsyncLifetime
    {
        private readonly ConcurrentDictionary<string, (string MemberNumber, string Token)> accountsByTier = new(StringComparer.OrdinalIgnoreCase);
        private readonly CosmosFixture cosmosFixture = new();

        public string Token { get; private set; } = string.Empty;
        public string MemberNumber { get; private set; } = string.Empty;

        public async Task InitializeAsync()
        {
            (string MemberNumber, string Token) redAccount = await GetAccountAsync("Red");
            MemberNumber = redAccount.MemberNumber;
            Token = redAccount.Token;
        }

        public async Task<(string MemberNumber, string Token)> GetAccountAsync(string tierCode)
        {
            if (accountsByTier.TryGetValue(tierCode, out (string MemberNumber, string Token) account))
            {
                return account;
            }

            string env = Config.Environment;

            // Shared non-production loyalty accounts support direct password sign-in for query coverage.
            Dictionary<string, string> loyaltyAccount = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints(tierCode);

            Assert.False(loyaltyAccount.ContainsKey("error"), loyaltyAccount.GetValueOrDefault("error"));
            Assert.True(loyaltyAccount.TryGetValue("rowKey", out string? memberNumber) && !string.IsNullOrWhiteSpace(memberNumber), $"Expected a member number for tier '{tierCode}' from the shared loyalty storage table.");
            Assert.True(loyaltyAccount.TryGetValue("email", out string? email) && !string.IsNullOrWhiteSpace(email), $"Expected an email address for tier '{tierCode}' from the shared loyalty storage table.");

            string storedMemberNumber = memberNumber ?? string.Empty;
            string token = await EntraTokenHelper.GetIdToken(env, email ?? string.Empty, storedMemberNumber, Application.LoginPassword);
            // Retain one safely shared account/token pair for each tier when test execution is parallel.
            return accountsByTier.GetOrAdd(tierCode, (storedMemberNumber, token));
        }

        public async Task<StoredLoyaltyMemberAccountDocuments> GetAccountDocumentsAsync(string tierCode)
        {
            (string MemberNumber, string Token) loyaltyAccount = await GetAccountAsync(tierCode);
            string env = Config.Environment;
            string customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            string customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            // Register lazily because the banner and flight tests share this fixture but do not need Cosmos data.
            cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            cosmosFixture.RegisterClient(Application.CustomerTransactionCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            cosmosFixture.RegisterType<FlyingClubMember>(Application.CustomerTransactionCosmosDbEndpoint, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            cosmosFixture.RegisterType<CustomerProfile>(Application.CustomerTransactionCosmosDbEndpoint, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            QueryDefinition customerQuery = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.LoyaltyMemberNumber = @memberNumber AND c.DocType = @docType")
                .WithParameter("@memberNumber", loyaltyAccount.MemberNumber)
                .WithParameter("@docType", Application.DocTypeCustomer);
            Customer customerDocument = Assert.IsType<Customer>(cosmosFixture.CosmosQueryItem<Customer>(customerQuery));

            QueryDefinition flyingClubMemberQuery = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
                .WithParameter("@customerId", customerDocument.CustomerId)
                .WithParameter("@docType", Application.DocTypeFlyingClubMember);
            FlyingClubMember flyingClubMemberDocument = Assert.IsType<FlyingClubMember>(cosmosFixture.CosmosQueryItem<FlyingClubMember>(flyingClubMemberQuery));

            QueryDefinition customerProfileQuery = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
                .WithParameter("@customerId", customerDocument.CustomerId)
                .WithParameter("@docType", Application.DocTypeCustomerProfile);
            CustomerProfile customerProfileDocument = Assert.IsType<CustomerProfile>(cosmosFixture.CosmosQueryItem<CustomerProfile>(customerProfileQuery));

            return new StoredLoyaltyMemberAccountDocuments(loyaltyAccount.MemberNumber, loyaltyAccount.Token, customerDocument, flyingClubMemberDocument, customerProfileDocument);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }

    public sealed record StoredLoyaltyMemberAccountDocuments(
        string MemberNumber,
        string Token,
        Customer CustomerDocument,
        FlyingClubMember FlyingClubMemberDocument,
        CustomerProfile CustomerProfileDocument);
}
