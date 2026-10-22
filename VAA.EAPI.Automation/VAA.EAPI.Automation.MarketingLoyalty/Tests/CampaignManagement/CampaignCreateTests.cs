using Newtonsoft.Json;
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
    public class CampaignCreateTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;

        public CampaignCreateTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(9)]
        public async Task A_CampaignCreate_Success()
        {
            string campaignCode = DataGenerator.RandomString(5).ToUpper();

            string startDate = DataGenerator.RandomPastDate(30).ToString("yyyy-MM-dd");

            string endDate = DataGenerator.RandomFutureDate(360).ToString("yyyy-MM-dd");

            string campaignName = $"TestCampaign{DataGenerator.RandomString(5).ToLower()}";

            string strQueryForCampaignType = "Select TOP 1 Value from dbo.ListOfValues where Type='CAMPAIGN_TYPE' and SubType='Standalone Campaign' and Value in ('Accrual','Fast Track to Gold','Flying Club Communication(CMD)','Silver After First Flight','Straight to Silver') order by NEWID()";

            string campaignType = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForCampaignType);

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), campaignCode, campaignName, startDate, endDate, "Active", campaignType, "Testing Purpose", true, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign created successfully";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse),
                $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

            string strQueryForCampaignTable = $"Select RowId from dbo.Campaigns where CampaignCode='{campaignCode}'";

            string campaignId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForCampaignTable);

            Assert.True(!string.IsNullOrEmpty(campaignId), "The Campaign is not Created in Reference Database.");

            strQueryForCampaignTable = $"delete from dbo.Campaigns where RowId='{campaignId}'";

            string dbResponse = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForCampaignTable);

            Assert.True(string.IsNullOrEmpty(dbResponse), "Campaign Deletion From Reference Database Table is Failed ");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(10)]
        public async Task B_CampaignCreate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignCodeIsNull()
        {
            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), "", "TestCm", "2022-12-12",
                "2050-12-12", "Active", "Accrual", "Testing Purpose", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(12)]
        public async Task C_CampaignCreate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignNameIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), "CODETEST", "",
                "2022-12-12", "2050-12-12", "Active", "Accrual", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse),
                $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(13)]
        public async Task D_CampaignCreate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignStartDateIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), "Test", "CampTest", "",
                "2050-12-12", "Active", "Accrual", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(14)]
        public async Task E_CampaignCreate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignEndDateIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), "Test", "CampTest",
                "2022-12-12", "", "Active", "Accrual", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(15)]
        public async Task F_CampaignCreate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignTypeIsNull()
        {
            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), "Test", "CampTest",
                "2022-12-12", "2050-10-11", "Active", "", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(16)]
        public async Task G_CampaignCreate_ShouldThrowInvalidCampaignTypeErrorWhenCampaignTypeIsInvalid()
        {
            string campaignCode = DataGenerator.RandomString(5).ToUpper();

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), campaignCode, "CampTest", "2022-12-12",
                "2050-10-11", "Active", "Straight to Red", "Description", true, "");

            string expectedResponse = "Invalid Campaign Type.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]

        [Fact, RunOrder(17)]
        public async Task H_CampaignCreate_ShouldThrowInvalidDateFormatErrorWhenStartDateIsInvalid()
        {

            string campaignCode = DataGenerator.RandomString(5).ToUpper();

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), campaignCode, "CampTest", "2022-13-12",
                "2050-10-11", "Active", "Other", "Description", true, "");

            string expectedResponse = "Invalid Date Formate.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]

        [Fact, RunOrder(18)]
        public async Task I_CampaignCreate_ShouldThrowInvalidDateFormatErrorWhenEndDateDateIsInvalid()
        {
            string campaignCode = DataGenerator.RandomString(5).ToUpper();

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), campaignCode, "CampTest", "2022-12-12",
                "2050-13-13", "Active", "Other", "Description", true, "");

            string expectedResponse = "Invalid Date Formate.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]

        [Fact, RunOrder(19)]
        public async Task J_CampaignCreate_ShouldThrowDuplicateCampaignCodeErrorWhenCampaignCodeAlreadyExistsInDatabase()
        {
            string strQueryForCampaignTable = "Select Top 1 CampaignCode from dbo.Campaigns order by StartDate desc";

            string campaignCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForCampaignTable);

            HttpResponseMessage response = GraphQL.CampaignCreate(tokenFixture.GetToken("graphqlv2"), campaignCode, "CampTest", "2022-12-12",
                "2050-10-10", "Active", "Other", "Description", true, "");

            string expectedResponse = "This Campaign Code is already used. Please use a different code.";

            dynamic createCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(createCampaignResult.ToString().Contains(expectedResponse), $"The Campaign Create response is not as expected. Expected {expectedResponse}  but was {createCampaignResult.ToString()}");

        }
    }
}
