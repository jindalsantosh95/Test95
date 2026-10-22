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
    public class CampaignRegisterTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        PreRequisiteTestData preRequisiteTestData;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public CampaignRegisterTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;

            this.cosmosFixture = cosmosFixture;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<TargetedMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<CampaignEnrollment>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            preRequisiteTestData = new PreRequisiteTestData();

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(20)]
        public async Task A_RegisterCampaignSuccess_WhenRegistrationRequiredFlagIsN()
        {

            #region Fetching the CustomerId and Campaign Details for Campaign Assignment

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = flyingClubMember.CustomerId;

            string strInputQueryForCampaignDetails = "SELECT TOP 1 RowId, CampaignCode FROM dbo.Campaigns WHERE Status='Active' AND RegistrationRequired='N' order by NEWID()";

            object[][] dbResponse = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForCampaignDetails);

            string campaignId = dbResponse[0][0]?.ToString();

            string campaignCode = dbResponse[0][1]?.ToString();

            string strInputQueryToVerifyPromotionMapping = $"SELECT PromotionId FROM dbo.CampaignsPromotionsMapping WHERE CampaignId='{campaignId}'";

            dbResponse = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryToVerifyPromotionMapping);

            string promotionId;

            try
            {
                promotionId = dbResponse[0].ToString();
            }
            catch (Exception)
            {
                promotionId = string.Empty;
            }
            #endregion

            #region Assigning the Campaign and validating the response and verifying the Database

            HttpResponseMessage response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", campaignCode, campaignId, "AutomationScript").EnsureIsSuccessStatusCode();

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Campaign assign initiated successfully";

            if (assignCampaignResult.ToString().Contains("Campaign Code Already Assigned"))
            {
                strInputQueryForCampaignDetails = "SELECT TOP 1 RowId, CampaignCode FROM dbo.Campaigns WHERE Status='Active' AND RegistrationRequired='N' ORDER BY NEWID()";

                dbResponse = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForCampaignDetails);

                campaignId = dbResponse[0][0]?.ToString();

                campaignCode = dbResponse[0][1]?.ToString();

                response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"),
                 customerId, "", campaignCode, campaignId, "AutomationScript").EnsureIsSuccessStatusCode();

                assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            }

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse} but was {assignCampaignResult.ToString()}");


            string queryForTargetedMembershipDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' AND c.PartitionKey='{customerId}' and c.ProfilingCampaignSourceId = '{campaignId}' order by c._ts desc";

            TargetedMembership targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            int retry = 5;

            while (retry > 0 && targetedMembership == null)
            {
                await Task.Delay(1000);

                targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

                retry--;
            }

            if (targetedMembership == null)
            {
                throw new Xunit.Sdk.XunitException("No Document is Available. Failed to retrieve the Cosmos Document");
            }

            string _campaignId = targetedMembership.ProfilingCampaignSourceId;

            string docId = targetedMembership.Id;

            string docStatus = targetedMembership.StatusCode;

            Assert.True(_campaignId.Equals(campaignId), $" Expected CampaignId is {campaignId} But Found {_campaignId}.");

            Assert.True(docStatus.Equals("Targeted"), $" Expected Doc Status is Targeted But Found {docStatus}.");

            #endregion

            #region Registering the Campaign After the Assignment

            response = GraphQL.RegisterCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", campaignCode, campaignId, docId, "AutomationScript").EnsureIsSuccessStatusCode();

            dynamic registerCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            expectedResponse = "Campaign registration initiated successfully";

            Assert.Contains(expectedResponse, registerCampaignResult.ToString());

            queryForTargetedMembershipDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' AND c.PartitionKey='{customerId}' AND c.id='{docId}' and c.StatusCode ='Registered' ORDER BY c._ts DESC";

            targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            retry = 5;

            while (retry > 0 && targetedMembership == null)
            {
                await Task.Delay(500);

                targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

                retry--;
            }

            if (targetedMembership == null)
            {
                throw new Xunit.Sdk.XunitException($"No Document is Available. Failed to retrieve the Cosmos Document with searched query {queryForTargetedMembershipDocument}.");
            }

            docStatus = targetedMembership.StatusCode;

            Assert.True(docStatus.Equals("Registered"), $" After Registration ,Expected Doc Status is Registered But Found {docStatus}.");

            #endregion

            #region Validation of Campaign Enrolment Document if the Campaign Assigned is linked to a promotion

            if (!string.IsNullOrEmpty(promotionId))
            {
                string queryForCampaignEnrolmentDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'CampaignEnrollment' AND c.PartitionKey='{customerId}' order by c._ts desc";

                CampaignEnrollment campaignEnrollment = cosmosFixture.QueryItem<CampaignEnrollment>(queryForCampaignEnrolmentDocument);

                retry = 5;

                while (retry > 0 && campaignEnrollment == null)
                {
                    await Task.Delay(500);

                    campaignEnrollment = cosmosFixture.QueryItem<CampaignEnrollment>(queryForCampaignEnrolmentDocument);

                    retry--;
                }

                if (campaignEnrollment != null)
                {
                    Assert.True(campaignEnrollment.PromotionId.Equals(promotionId),
                        $"Expected the CampaignEnrolment Document with Promotion {promotionId} But Found {campaignEnrollment.PromotionId}");
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("CampaignEnrollment Document is not Created.");
                }
            }
            #endregion
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(21)]
        public async Task B_RegisterCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCustomerIdIsNull()
        {

            HttpResponseMessage response = GraphQL.RegisterCampaign(tokenFixture.GetToken("graphqlv2"), "", "", "MS02", "",
                "794cd4a7-a83e-46fc-b6c0-c44587c0ced3", "testp");

            dynamic registerCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "\"Unhandled exception (Either 'Customer ID' or 'Member Number' must be specified)\"";

            Assert.True(registerCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for Register Campaign is not as expected. Expected {expectedResponse} but was {registerCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(22)]
        public async Task C_RegisterCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCampaignIdIsNull()
        {
            string customerId;

            preRequisiteTestData.GetValidFCNumber(out customerId);

            HttpResponseMessage response = GraphQL.RegisterCampaign(tokenFixture.GetToken("graphqlv2"), customerId, "", "MS02", "", "794cd4a7-a83e-46fc-b6c0-c44587c0ced3", "testp");

            dynamic registerCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = $"One of the mandatory field is missing in the request.";

            Assert.True(registerCampaignResult.ToString().Contains(expectedResponse), $"Text value for Register Campaign is not as expected. Expected {expectedResponse} but was {registerCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(23)]
        public async Task D_RegisterCampaign_ShouldThrowMandatoryFieldMissingErrorWhenCampaignCodeIsNull()
        {

            string customerId;

            preRequisiteTestData.GetValidFCNumber(out customerId);

            HttpResponseMessage response = GraphQL.RegisterCampaign(tokenFixture.GetToken("graphqlv2"), customerId, "", "", "1-F8-41", "794cd4a7-a83e-46fc-b6c0-c44587c0ced3", "testp");

            dynamic registerCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = $"One of the mandatory field is missing in the request.";

            Assert.True(registerCampaignResult.ToString().Contains(expectedResponse), $"Text value for Register Campaign is not as expected. Expected {expectedResponse} but was {registerCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(24)]
        public async Task E_RegisterCampaignSuccess_WhenRegistrationRequiredFlagIsY()
        {

            #region Fetching the CustomerId and Campaign Details for Campaign Assignment

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = flyingClubMember.CustomerId;

            string strInputQueryForCampaignDetails = "SELECT TOP 1 RowId, CampaignCode FROM dbo.Campaigns WHERE Status='Active' AND RegistrationRequired='Y' order by NEWID()";

            object[][] dbResponse = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForCampaignDetails);

            string campaignId = dbResponse[0][0]?.ToString();

            string campaignCode = dbResponse[0][1]?.ToString();

            string strInputQueryToVerifyPromotionMapping = $"SELECT PromotionId FROM dbo.CampaignsPromotionsMapping WHERE CampaignId='{campaignId}'";

            dbResponse = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryToVerifyPromotionMapping);

            string promotionId;

            try
            {
                promotionId = dbResponse[0][0]?.ToString();
            }
            catch (Exception)
            {
                promotionId = string.Empty;
            }
            #endregion

            #region Assigning the Campaign and validating the response and verifying the Database

            HttpResponseMessage response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"),
                customerId, "", campaignCode, campaignId, "AutomationScript").EnsureIsSuccessStatusCode();

            dynamic assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Campaign assign initiated successfully";

            if (assignCampaignResult.ToString().Contains("Campaign Code Already Assigned"))
            {
                strInputQueryForCampaignDetails = "SELECT TOP 1 RowId, CampaignCode FROM dbo.Campaigns WHERE Status='Active' AND RegistrationRequired='N' ORDER BY NEWID()";

                dbResponse = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForCampaignDetails);

                campaignId = dbResponse[0][0]?.ToString();

                campaignCode = dbResponse[0][1]?.ToString();

                response = GraphQL.AssignCampaign(tokenFixture.GetToken("graphqlv2"),
                 customerId, "", campaignCode, campaignId, "AutomationScript").EnsureIsSuccessStatusCode();

                assignCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            }

            Assert.True(assignCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for assign Campaign is not as expected. Expected {expectedResponse} but was {assignCampaignResult.ToString()}");


            string queryForTargetedMembershipDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'TargetedMembership' AND c.PartitionKey='{customerId}' and c.ProfilingCampaignSourceId = '{campaignId}' order by c._ts desc";

            TargetedMembership targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

            int retry = 5;

            while (retry > 0 && targetedMembership == null)
            {
                await Task.Delay(1000);

                targetedMembership = cosmosFixture.QueryItem<TargetedMembership>(queryForTargetedMembershipDocument);

                retry--;
            }

            if (targetedMembership == null)
            {
                throw new Xunit.Sdk.XunitException("No Document is Available. Failed to retrieve the Cosmos Document");
            }

            string _campaignId = targetedMembership.ProfilingCampaignSourceId;

            string docStatus = targetedMembership.StatusCode;

            Assert.True(_campaignId.Equals(campaignId), $" Expected CampaignId is {campaignId} But Found {_campaignId}.");

            Assert.True(docStatus.Equals("Registered"), $" Expected Doc Status is Registered But Found {docStatus}.");

            #endregion

            #region Validation of Campaign Enrolment Document if the Campaign Assigned is linked to a promotion

            if (!string.IsNullOrEmpty(promotionId))
            {
                string queryForCampaignEnrolmentDocument = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'CampaignEnrollment' AND c.PartitionKey='{customerId}' order by c._ts desc";

                CampaignEnrollment campaignEnrollment = cosmosFixture.QueryItem<CampaignEnrollment>(queryForCampaignEnrolmentDocument);

                retry = 5;

                while (retry > 0 && campaignEnrollment == null)
                {
                    await Task.Delay(500);

                    campaignEnrollment = cosmosFixture.QueryItem<CampaignEnrollment>(queryForCampaignEnrolmentDocument);

                    retry--;
                }

                if (campaignEnrollment != null)
                {
                    Assert.True(campaignEnrollment.PromotionId.Equals(promotionId),
                        $"Expected the CampaignEnrolment Document with Promotion {promotionId} But Found {campaignEnrollment.PromotionId}");
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("CampaignEnrollment Document is not Created.");
                }
            }
            #endregion
        }
    }
}
