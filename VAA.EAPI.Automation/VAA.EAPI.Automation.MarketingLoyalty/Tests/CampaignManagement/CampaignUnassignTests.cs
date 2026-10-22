using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CampaignManagement
{

    [Collection("CampaignManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CampaignUnassignTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public CampaignUnassignTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(41)]
        public async Task A_UnassignCampaignSuccess()
        {

            string queryForTargetedMembershipDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' order by c.CreatedOn desc";

            TargetedMembership targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            string customerId = targetedMembership.CustomerId;

            string assignedBy = "GraphQlTestScript" + DataGenerator.RandomString(5);

            string currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string queryForCampaignDetails = $"Select TOP 1 CampaignCode,RowId from dbo.Campaigns where Status='Active' and StartDate <= '{currentDate}' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campCode = campaignDetails[0][0]?.ToString();

            string campId = campaignDetails[0][1]?.ToString();

            HttpResponseMessage response = GraphQL.RetrieveMemberCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", "", "").EnsureIsSuccessStatusCode();

            dynamic memberCampaignRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            dynamic memberCampaignList = memberCampaignRetrieveResult.data.loyaltyMemberCampaigns;

            if (!string.IsNullOrEmpty(memberCampaignList.ToString()))
            {

                foreach (dynamic code in memberCampaignList)
                {
                    if (code.campaignCode.ToString().Equals(campCode))
                    {
                        dynamic targetedDocumentId = code.id.ToString();

                        response = GraphQL.UnassignCampaign(tokenFixture.GetToken("graphqlv2"),
                                           customerId, targetedDocumentId).EnsureIsSuccessStatusCode();

                        dynamic unassignCampaignEndpointResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                        string unassignCampaignExpectedResponseText = "Un-assign campaign completed successfully";

                        Assert.True(unassignCampaignEndpointResult.data.loyaltyUnassignCampaign.ToString() == unassignCampaignExpectedResponseText,
                            $"Text value for unassign Campaign is not as expected. Expected {unassignCampaignExpectedResponseText} " +
                            $"but was {unassignCampaignEndpointResult.data.loyaltyUnassignCampaign.ToString()}");

                    }
                }
            }

            HttpResponseMessage assignCampaignResponse = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", campCode, campId, assignedBy).EnsureIsSuccessStatusCode();

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await assignCampaignResponse.Content.ReadAsStringAsync());

            string expectedResponse = "Campaign assign initiated successfully";

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse} but was {assignCampaignResult.data.loyaltyAssignCampaign.ToString()}");

            queryForTargetedMembershipDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' and c.PartitionKey='{customerId}' and c.ProfilingCampaignSourceId = '{campId}' order by c._ts desc";

            targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            int retry = 5;

            while (retry > 0 && targetedMembership == null)
            {
                await Task.Delay(500);

                targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

                retry--;
            }

            Assert.True(targetedMembership is not null,
                $" After Campaign Assign, No Document Available for the Searched Query {queryForTargetedMembershipDocument}");

            string createdBy = targetedMembership.CreatedBy;

            string lastUpdatedBy = targetedMembership.LastUpdatedBy;

            string _campaignId = targetedMembership.ProfilingCampaignSourceId;

            string doc_Id = targetedMembership.Id;

            Assert.True(_campaignId.Equals(campId), $" Expected CampaignId is {campId} But Found {_campaignId}.");

            Assert.True(createdBy.Equals(assignedBy), $" Expected CreatedBy is {assignedBy} But Found {createdBy}.");

            Assert.True(lastUpdatedBy.Equals(assignedBy), $" Expected LastUpdatedBy is {assignedBy} But Found {lastUpdatedBy}.");

            HttpResponseMessage retrieveCampaignResponse = GraphQL.RetrieveMemberCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", "", "").EnsureIsSuccessStatusCode();

            dynamic retrieveCampaignResult = JsonConvert.DeserializeObject<dynamic>(await retrieveCampaignResponse.Content.ReadAsStringAsync());

            string documentId = retrieveCampaignResult.data.loyaltyMemberCampaigns[0].id.ToString();

            Assert.True(retrieveCampaignResult.data.loyaltyMemberCampaigns.ToString().Contains(doc_Id),
                $"Expected Document Id is {doc_Id} but not found.");

            response = GraphQL.UnassignCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, documentId).EnsureIsSuccessStatusCode();

            dynamic unassignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string unassignCampaignExpectedResponse = "Un-assign campaign completed successfully";

            Assert.True(unassignCampaignResult.ToString().Contains(unassignCampaignExpectedResponse),
                $"Text value for unassign Campaign is not as expected. Expected {unassignCampaignExpectedResponse} but was {unassignCampaignResult.ToString()}");

            HttpResponseMessage retrieveCampaignResponse2 = GraphQL.RetrieveMemberCampaign(tokenFixture.GetToken("graphqlv2"),
               customerId, "", "", "").EnsureIsSuccessStatusCode();

            dynamic retrieveCampaignResult2 = JsonConvert.DeserializeObject<dynamic>(await retrieveCampaignResponse2.Content.ReadAsStringAsync());

            Assert.True(!retrieveCampaignResult2.ToString().Contains(documentId),
              $"Did not Expect Document Id {documentId} in the response but found.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(42)]
        public async Task B_UnassignCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCustomerIdIsNull()
        {

            HttpResponseMessage response = GraphQL.UnassignCampaign(tokenFixture.GetToken("graphqlv2"), "",
                "b277a37e-64cc-4071-9f1b-f5319e064902");

            dynamic unassignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "('CustomerId' And DocumentId must be spefied.)";

            Assert.True(unassignCampaignResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Text value for Campaign unassign is not as expected. Expected {expectedResponse} but was {unassignCampaignResult.errors[0].message.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(43)]
        public async Task C_UnassignCampaign_ShouldThrowMandatoryFieldMissingErrorWhenDocumentIdIsNull()
        {

            HttpResponseMessage response = GraphQL.UnassignCampaign(tokenFixture.GetToken("graphqlv2"),
                "fc1f8480-7c95-44a4-9fd0-06ba9f4deafce", "");

            dynamic unassignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "('CustomerId' And DocumentId must be spefied.)";

            Assert.True(unassignCampaignResult.errors[0].message.ToString().Contains(expectedResponse),
                $"Text value for Campaign unassign is not as expected. Expected {expectedResponse} but was {unassignCampaignResult.errors[0].message.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(44)]
        public async Task D_UnassignCampaign_ShouldThrowInvalidFieldErrorWhenCustomerIdIsIncorrect()
        {

            HttpResponseMessage response = GraphQL.UnassignCampaign(tokenFixture.GetToken("graphqlv2"), "4dcf9ca9-4c79-4b3b-8a82",
                "43d41b69-8662-4f08-83b2-e416bd76a1a5");

            dynamic unassignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "There was an unexpected error.";

            Assert.True(unassignCampaignResult.errors[0].extensions.message.ToString().Contains(expectedResponse),
                $"Text value for Campaign unassign is not as expected. Expected {expectedResponse} but was {unassignCampaignResult.errors[0].extensions.message.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(45)]
        public async Task E_UnassignCampaign_ShouldThrowInvalidFieldErrorWhenDocumentIdIsIncorrect()
        {
            HttpResponseMessage response = GraphQL.UnassignCampaign(tokenFixture.GetToken("graphqlv2"),
                        "4dcf9ca9-4c79-4b3b-8a82-7013e015e963", "43d41b69-8662-4f08-83b2");

            dynamic unassignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "There was an unexpected error.";

            Assert.True(unassignCampaignResult.errors[0].extensions.message.ToString().Contains(expectedResponse),
                $"Text value for Campaign unassign is not as expected. Expected {expectedResponse} but was {unassignCampaignResult.errors[0].extensions.message.ToString()}");

        }


    }
}
