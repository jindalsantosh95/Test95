using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.CustomerTransaction
{
    public class RetrieveMembershipChangeDocTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveMembershipChangeDocTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");
        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact]
        public async Task A_RetrieveMembershipChangeDoc_SuccessScenario()
        {
            #region Test Data

            string queryForMembershipChangeDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForMembershipChangeDoc).WithParameter("@docType", "MembershipChange").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            MembershipChange membershipChange = cosmosFixture.CosmosQueryItem<MembershipChange>(parameterizedQuery);

            string customerId = membershipChange.CustomerId;

            #endregion

            HttpResponseMessage response = Microservice.RetrieveMembershipChangeDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrieveMembershipChangeDocResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(retrieveMembershipChangeDocResult.customerId.ToString().Equals(customerId), $"Expected the CustomerId as {customerId}  but found {retrieveMembershipChangeDocResult.customerId}");

            dynamic result = retrieveMembershipChangeDocResult.membershipChangeDocuments[0];

            Assert.True(result.active.ToString().Equals(membershipChange.Active), $"Expected the Doc Active Flag as {membershipChange.Active} but found {result.active.ToString()}.");

            Assert.True(DateTimeOffset.Parse(result.endDate.ToString()).Equals(membershipChange.EndDate), $"Expected the Active Doc EndDate as {membershipChange.EndDate} but found {result.endDate.ToString()}.");

            Assert.True(result.memberNumber.ToString().Equals(membershipChange.MemberNumber), $"Expected the MemberNumber in the Doc as {membershipChange.MemberNumber} but found {result.memberNumber.ToString()}.");

            Assert.True(result.startDate.ToString("yyyy-MM-dd").Equals(membershipChange.StartDate?.ToString("yyyy-MM-dd")), $"Expected the Active Doc StartDate  as {membershipChange.StartDate} but found {result.startDate.ToString()}.");

            Assert.True(result.tierCode.ToString().Equals(membershipChange.TierCode), $"Expected the Active Doc TierCode  as {membershipChange.TierCode} but found {result.tierCode.ToString()}.");

            Assert.True(result.createdBy.ToString().Equals(membershipChange.CreatedBy), $"Expected the Active Doc CreatedBy as {membershipChange.CreatedBy} but found {result.createdBy.ToString()}.");

            Assert.True(result.lastUpdatedBy.ToString().Equals(membershipChange.LastUpdatedBy), $"Expected the Active Doc LastUpdatedBy as {membershipChange.LastUpdatedBy} but found {result.lastUpdatedBy.ToString()}.");
        }
    }
}
