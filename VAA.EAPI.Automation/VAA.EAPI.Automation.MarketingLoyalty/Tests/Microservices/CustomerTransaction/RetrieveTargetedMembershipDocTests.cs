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
    public class RetrieveTargetedMembershipDocTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveTargetedMembershipDocTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");
        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact]
        public async Task A_RetrieveTargetedMembershipDoc_SuccessScenario()
        {
            #region Test Data

            string queryForTargetedMembershipDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType  order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTargetedMembershipDoc).WithParameter("@docType", "TargetedMembership");

            TargetedMembership targetedMembership = cosmosFixture.CosmosQueryItem<TargetedMembership>(parameterizedQuery);

            string customerId = targetedMembership.CustomerId;

            #endregion

            HttpResponseMessage response = Microservice.RetrieveTargetedMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrieveTargetedMembershipDocResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            dynamic result = retrieveTargetedMembershipDocResult.targetedMembershipDocuments[0];

            Assert.True(result.customerId.ToString().Equals(customerId), $"Expected the CustomerId as {customerId}  but found {result.customerId}");

            Assert.True(result.profilingCampaignSourceId.ToString().Equals(targetedMembership.ProfilingCampaignSourceId), $"Expected the Campaign Id as {targetedMembership.ProfilingCampaignSourceId} but found {result.profilingCampaignSourceId.ToString()}.");

            Assert.True(result.statusCode.ToString().Equals(targetedMembership.StatusCode), $"Expected the Document Status as {targetedMembership.StatusCode} but found {result.statusCode.ToString()}.");

            Assert.True(result.targetedMembershipId.ToString().Equals(targetedMembership.TargetedMembershipId), $"Expected the Document Id as {targetedMembership.TargetedMembershipId} but found {result.targetedMembershipId.ToString()}.");

            Assert.True(DateTimeOffset.Parse(result.completedDate.ToString()).Equals(targetedMembership.CompletedDate), $"Expected the Document Completed Date as {targetedMembership.CompletedDate} but found {result.contactedDate.ToString()}.");

            Assert.True(result.createdBy.ToString().Equals(targetedMembership.CreatedBy), $"Expected the Document CreatedBy as {targetedMembership.CreatedBy} but found {result.createdBy.ToString()}.");

            Assert.True(result.lastUpdatedBy.ToString().Equals(targetedMembership.LastUpdatedBy), $"Expected the Document LastUpdateBy as {targetedMembership.LastUpdatedBy} but found {result.lastUpdatedBy.ToString()}.");

        }
    }
}
