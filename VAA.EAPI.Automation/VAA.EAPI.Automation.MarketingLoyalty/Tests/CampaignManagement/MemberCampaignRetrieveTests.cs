using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CampaignManagement
{
    [Collection("CampaignManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MemberCampaignRetrieveTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public MemberCampaignRetrieveTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(75)]
        public async Task A_RetrieveMemberCampaignSuccess()
        {

            #region - Pre requisite - Fetching CustomerId from targeted Membership Document

            string queryForTargetedMembershipDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' order by c._ts desc";

            TargetedMembership targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            string customerId = targetedMembership.CustomerId;

            queryForTargetedMembershipDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' AND c.PartitionKey='{customerId}' order by c._ts desc";

            IEnumerable<TargetedMembership> targetedMembershipDocs = cosmosFixture.QueryItems<TargetedMembership>(queryForTargetedMembershipDocument);

            int count = targetedMembershipDocs.Count();

            #endregion

            HttpResponseMessage response = GraphQL.RetrieveMemberCampaign(tokenFixture.GetToken("graphqlv2"), customerId, "", "", "").EnsureIsSuccessStatusCode();

            dynamic RetrieveMemberCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = customerId;

            Assert.True(RetrieveMemberCampaignResult.ToString().Contains(expectedResponse),
                $"Expected the Cust Id in the response as {expectedResponse} but found {RetrieveMemberCampaignResult.ToString()}");

            Assert.True(RetrieveMemberCampaignResult.data.loyaltyMemberCampaigns.Count.Equals(count),
                $"Expected the count to be {count} but found {RetrieveMemberCampaignResult.data.loyaltyMemberCampaigns.Count}.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(76)]
        public async Task B_RetrieveMemberCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCustomerIdIsNull()
        {

            HttpResponseMessage response = GraphQL.RetrieveMemberCampaign(tokenFixture.GetToken("graphqlv2"), "", "", "", "");

            dynamic RetrieveMemberCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Unhandled exception (Either 'customerId' or memberId must be spefied.)";

            Assert.True(RetrieveMemberCampaignResult.ToString().Contains(expectedResponse),
                $"Expected the Cust Id in the response as {expectedResponse} but found {RetrieveMemberCampaignResult.ToString()}");

        }
    }
}
