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
    public class CampaignUpdateTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;

        public CampaignUpdateTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        Random random = new();

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(46)]
        public async Task A_CampaignCodeUpdate_Success()
        {

            string campaignCode = DataGenerator.RandomString(5).ToUpper();

            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignName = campaignDetails[0][2]?.ToString();
            string startDate = campaignDetails[0][3]?.ToString();
            string endDate = campaignDetails[0][4]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][6]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";
            string description = campaignDetails[0][8]?.ToString();

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, status, type, description, regReqFlag, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"Select CampaignCode from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            string campCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate);

            Assert.True(campCode.Equals(campaignCode), $"The CampaignCode is not as expected. Expected {campaignCode} but found {campCode}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(47)]
        public async Task B_CampaignNameUpdate_Success()
        {

            string campaignName = DataGenerator.RandomString(5);

            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignCode = campaignDetails[0][1]?.ToString();
            string startDate = campaignDetails[0][3]?.ToString();
            string endDate = campaignDetails[0][4]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][6]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";
            string description = campaignDetails[0][8]?.ToString();

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, status, type, description, regReqFlag, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"Select Name from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            string campName = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate);

            Assert.True(campName.Equals(campaignName), $"The CampaignName is not as expected. Expected {campaignName} but found {campName}");


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(48)]
        public async Task C_CampaignStartDateUpdate_Success()
        {
            string startDate = DataGenerator.RandomPastDate().ToString("MM/dd/yyyy");

            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignCode = campaignDetails[0][1]?.ToString();
            string campaignName = campaignDetails[0][2]?.ToString();
            string endDate = campaignDetails[0][4]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][6]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";
            string description = campaignDetails[0][8]?.ToString();

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, status, type, description, regReqFlag, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"select StartDate from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            string campStartDate = ((DateTime)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate)).ToString("MM/dd/yyyy");

            Assert.True(campStartDate.Equals(startDate), $"The CampaignStartDate is not as expected. Expected {startDate} but found {campStartDate}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(49)]
        public async Task D_CampaignEndDateUpdate_Success()
        {
            string endDate = DataGenerator.RandomFutureDate().ToString("MM/dd/yyyy");

            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignCode = campaignDetails[0][1]?.ToString();
            string campaignName = campaignDetails[0][2]?.ToString();
            string startDate = campaignDetails[0][3]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][6]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";
            string description = campaignDetails[0][8]?.ToString();

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, status, type, description, regReqFlag, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
               $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"select EndDate from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            string campEndDate = ((DateTime)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate)).ToString("MM/dd/yyyy");

            Assert.True(campEndDate.Equals(endDate), $"The CampaignEndDate is not as expected. Expected {endDate} but found {campEndDate}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(50)]
        public async Task E_CampaignStatusUpdate_Success()
        {
            string[] validCampaignStatus = { "Active", "Cancelled", "In Planning" };

            int i = random.Next(validCampaignStatus.Length - 1);

            string campainStatus = validCampaignStatus[i];

            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignCode = campaignDetails[0][1]?.ToString();
            string campaignName = campaignDetails[0][2]?.ToString();
            string startDate = campaignDetails[0][3]?.ToString();
            string endDate = campaignDetails[0][4]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][5]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";
            string description = campaignDetails[0][8]?.ToString();

            while (campainStatus == status)
            {
                i = random.Next(validCampaignStatus.Length - 1);
                campainStatus = validCampaignStatus[i];
            }

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, campainStatus, type, description, regReqFlag, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"select Status from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            string campStatus = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate);

            Assert.True(campStatus.Equals(campainStatus), $"The Campaign Status is not as expected. Expected {campainStatus} but found {campStatus}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(51)]
        public async Task F_CampaignDescriptionUpdate_Success()
        {
            string campaignDescription = DataGenerator.RandomString(30);

            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignCode = campaignDetails[0][1]?.ToString();
            string campaignName = campaignDetails[0][2]?.ToString();
            string startDate = campaignDetails[0][3]?.ToString();
            string endDate = campaignDetails[0][4]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][6]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, status, type, campaignDescription, regReqFlag, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
               $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"select Description from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            string campDescription = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate);

            Assert.True(campaignDescription.Equals(campDescription),
                $"The Campaign Description is not as expected. Expected {campaignDescription} but found {campDescription}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(52)]
        public async Task G_CampaignRegReqFlagUpdate_Success()
        {
            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignCode = campaignDetails[0][1]?.ToString();
            string campaignName = campaignDetails[0][2]?.ToString();
            string startDate = campaignDetails[0][3]?.ToString();
            string endDate = campaignDetails[0][4]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][6]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";
            string description = campaignDetails[0][8]?.ToString();

            bool campaignRegReqFlag = !regReqFlag;

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, status, type, description, campaignRegReqFlag, "").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
               $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"select RegistrationRequired from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            bool campRegReqFlag = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate) == "Y";

            Assert.True(campRegReqFlag.Equals(campaignRegReqFlag), $"The Campaign Registration Required Flag" +
                                        $" is not as expected. Expected {campaignRegReqFlag} but found {campRegReqFlag}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(53)]
        public async Task H_CampaignLinkToTemplateUpdate_Success()
        {
            string campaignLinkToTemplate = DataGenerator.RandomString(30);

            string queryForCampaignDetails = "Select TOP 1 RowId,CampaignCode,Name,StartDate,EndDate,Type,Status,RegistrationRequired,Description from dbo.Campaigns where Status='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignDetails);

            string campaignId = campaignDetails[0][0]?.ToString();
            string campaignCode = campaignDetails[0][1]?.ToString();
            string campaignName = campaignDetails[0][2]?.ToString();
            string startDate = campaignDetails[0][3]?.ToString();
            string endDate = campaignDetails[0][4]?.ToString();
            string type = campaignDetails[0][5]?.ToString();
            string status = campaignDetails[0][6]?.ToString();
            bool regReqFlag = campaignDetails[0][7]?.ToString() == "Y";
            string description = campaignDetails[0][8]?.ToString();

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), campaignId, campaignCode,
                campaignName, startDate, endDate, status, type, description, regReqFlag, campaignLinkToTemplate).EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign details updated successfully";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(updateCampaignResult.ToString().Contains(expectedResponse),
            $"Text value for note is not as expected. Expected {expectedResponse}  but was {updateCampaignResult.ToString()}");

            string strInputQueryForCampaignUpdate = $"select LinkToTemplate from [dbo].[Campaigns]  where rowID = '{campaignId}'";

            string campLinkToTemplate = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForCampaignUpdate);

            Assert.True(campaignLinkToTemplate.Equals(campLinkToTemplate),
                                     $"The Campaign Link to template is not as expected. Expected {campaignLinkToTemplate} " +
                                      $"but found {campLinkToTemplate}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(54)]
        public async Task I_CampaignUpdate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignCodeIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "", "TestCm",
                "2022-12-12", "2050-12-12", "Active", "Accrual", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(55)]
        public async Task J_CampaignUpdate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignNameIsNull()
        {
            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "Test", "",
                "2022-12-12", "2050-12-12", "Active", "Accrual", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(56)]
        public async Task K_CampaignUpdate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignStartDateIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "Test",
                            "CampTest", "", "2050-12-12", "Active", "Accrual", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(57)]
        public async Task L_CampaignUpdate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignEndDateIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "Test",
                "CampTest", "2022-12-12", "", "Active", "Accrual", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(58)]
        public async Task M_CampaignUpdate_ShouldThrowMandatoryFieldMissingErrorWhenCampaignTypeIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "Test",
                "CampTest", "2022-12-12", "2050-10-11", "Active", "", "Description", true, "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(59)]
        public async Task N_CampaignUpdate_ShouldThrowInvalidCampaignIdErrorWhenCampaignIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-ABCD",
                "Test", "CampTest", "2022-12-12", "2050-10-11", "Active", "Other", "Description", true, "");

            string expectedResponse = "Invalid Campaign Id.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(60)]
        public async Task O_CampaignUpdate_ShouldThrowInvalidDateFormatErrorWhenStartDateIsInvalid()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "Test",
                "CampTest", "2022-13-12", "2050-10-11", "Active", "Other", "Description", true, "");

            string expectedResponse = "Invalid Date Formate.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(61)]
        public async Task P_CampaignUpdate_ShouldThrowInvalidDateFormatErrorWhenEndDateDateIsInvalid()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "Test",
                "CampTest", "2022-12-12", "2050-13-13", "Active", "Other", "Description", true, "");

            string expectedResponse = "Invalid Date Formate.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(62)]
        public async Task Q_CampaignUpdate_ShouldThrowDuplicateCampaignCodeErrorWhenCampaignCodeAlreadyExistsInDatabase()
        {

            HttpResponseMessage response = GraphQL.CampaignUpdate(tokenFixture.GetToken("graphqlv2"), "1-F3-4726", "WELS462",
                "CampTest", "2022-12-12", "2050-10-10", "Active", "Other", "Description", true, "");

            string expectedResponse = "This Campaign Code is already used. Please use a different code.";

            dynamic updateCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.Contains(expectedResponse, updateCampaignResult.ToString());

        }
    }
}
