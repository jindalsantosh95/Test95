using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CampaignManagement
{
    [Collection("CampaignManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CampaignAssignTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        PreRequisiteTestData preRequisiteTestData;

        public string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public CampaignAssignTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            preRequisiteTestData = new PreRequisiteTestData();

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task A_AssignCampaignSuccess()
        {
            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string queryForCampaignDetails = $"Select TOP 1 CampaignCode,RowId from dbo.Campaigns where Status='Active' and StartDate <= '{currentDate}' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignCode = campaignDetails[0][0]?.ToString();

            string campaignId = campaignDetails[0][1]?.ToString();

            HttpResponseMessage response = GraphQL.RetrieveMemberCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", "", "").EnsureIsSuccessStatusCode();

            dynamic memberCampaignRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            dynamic memberCampaignList = memberCampaignRetrieveResult.data.loyaltyMemberCampaigns;

            if (!string.IsNullOrEmpty(memberCampaignList.ToString()))
            {

                foreach (dynamic code in memberCampaignList)
                {
                    if (code.campaignCode.ToString().Equals(campaignCode))
                    {
                        dynamic documentId = code.id.ToString();

                        response = GraphQL.UnassignCampaign(tokenFixture.GetToken("graphqlv2"), customerId, documentId).EnsureIsSuccessStatusCode();

                        dynamic unassignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                        string unassignCampaignExpectedResponse = "Un-assign campaign completed successfully";

                        Assert.True(unassignCampaignResult.ToString().Contains(unassignCampaignExpectedResponse),
                            $"Text value for unassign Campaign is not as expected. Expected {unassignCampaignExpectedResponse} " +
                            $"but was {unassignCampaignResult.ToString()}");

                    }
                }
            }

            response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", campaignCode, campaignId, "testp").EnsureIsSuccessStatusCode();

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Campaign assign initiated successfully";

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse}" +
                $" but was {assignCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task B_AssignCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCustomerIdIsNull()
        {
            HttpResponseMessage response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"), "", "", "SALE822", "1-3BFJ-15", "testp");

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "\"Unhandled exception (Either 'customerId' or memberNumber must be spefied.)\"";

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse}" +
                $" but was {assignCampaignResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(5)]
        public async Task C_AssignCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCampaignIdIsNull()
        {
            string customerId;

            preRequisiteTestData.GetValidFCNumber(out customerId);

            HttpResponseMessage response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"), customerId, "", "SALE822", "", "testp");

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse}" +
                $" but was {assignCampaignResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task D_AssignCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCampaignCodeIsNull()
        {
            string customerId;

            preRequisiteTestData.GetValidFCNumber(out customerId);

            HttpResponseMessage response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"), customerId, "", "", "MS02", "testp");

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse}" +
                $" but was {assignCampaignResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(7)]
        public async Task E_AssignCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCreatedByIsNull()
        {
            string customerId;

            preRequisiteTestData.GetValidFCNumber(out customerId);

            HttpResponseMessage response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"), customerId, "", "CMDTEST", "1679664513289", "");

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse}" +
                $" but was {assignCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(8)]
        public async Task F_AssignCampaign_ShouldThrowDuplicateCampaignCodeWhenCampaignCodeIsAlreadyAssigned()
        {


            string queryForTargetedMembershipDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTargetedMembershipDoc).WithParameter("@docType", "TargetedMembership");

            TargetedMembership fcmDoc = cosmosFixture.CosmosQueryItem<TargetedMembership>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string campaignId = fcmDoc.ProfilingCampaignSourceId;

            string queryForCampaignData = $"Select CampaignCode from dbo.Campaigns where RowId='{campaignId}'";

            string campaignCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignData);

            HttpResponseMessage response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"), customerId, "", campaignCode, campaignId, "");

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Campaign Code Already Assigned";

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse}" +
                $" but was {assignCampaignResult.ToString()}");
        }
    }
}
