using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.PromotionManagement
{
    [Collection("PromotionManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class PromotionUpdateTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public PromotionUpdateTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        Random random = new();

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(15)]
        public async Task A_PromotionNameUpdate_Success()
        {

            string strInputQueryForPromotionSearch = "Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.StartDate is not null and PR.EndDate is not null and PP.Name is not null order by NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            string promotionId = promotionDetails[0][0]?.ToString();

            string promotionStartDate = DateTime.Parse(promotionDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDate = DateTime.Parse(promotionDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlag = promotionDetails[0][4]?.ToString();

            string promPartnerId = promotionDetails[0][5]?.ToString();

            string promPartnerName = promotionDetails[0][6]?.ToString();

            string promotionNameNew = DataGenerator.RandomString(10);

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), promotionId,
                promotionNameNew, promPartnerName, promotionStartDate, promotionEndDate, promotionActiveFlag);

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionUpdateResult.data?.loyaltyUpdatePromotion?.ToString() == "Promotion details updated successfully",
                $"Expected the promotion Name Update to be successful for the {promotionId} but found {promotionUpdateResult}");

            int retry = 5;

            strInputQueryForPromotionSearch = $"Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.Name='{promotionNameNew}' and PR.RowId='{promotionId}' ";

            object[][] promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            while (promotionDetailsNew.Length == 0 && retry > 0)
            {
                await Task.Delay(1000);

                promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

                retry--;
            }


            string promotionNameAfterUpdate = promotionDetailsNew[0][1]?.ToString();

            string promotionStartDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlagAfterUpdate = promotionDetailsNew[0][4]?.ToString();

            string promPartnerIdAfterUpdate = promotionDetailsNew[0][5]?.ToString();


            Assert.True(promotionNameAfterUpdate.Equals(promotionNameNew),
                $"When PromotionName is Updated ,The Promotion Name is not as expected. Expected value {promotionNameNew} but found {promotionNameAfterUpdate}");

            Assert.True(promotionStartDateAfterUpdate.Equals(promotionStartDate),
               $"When PromotionName is Updated ,The Promotion StartDate is not as expected. Expected value {promotionStartDate} but found {promotionStartDateAfterUpdate}");

            Assert.True(promotionEndDateAfterUpdate.Equals(promotionEndDate),
               $"When PromotionName is Updated ,The Promotion EndDate is not as expected. Expected value {promotionEndDate} but found {promotionEndDateAfterUpdate}");

            Assert.True(promotionActiveFlagAfterUpdate.Equals(promotionActiveFlag),
               $"When PromotionName is Updated ,The Promotion Active Flag is not as expected. Expected value {promotionActiveFlag} but found {promotionActiveFlagAfterUpdate}");

            Assert.True(promPartnerIdAfterUpdate.Equals(promPartnerId),
              $"When PromotionName is Updated ,The Promotion Partner Id is not as expected. Expected value {promPartnerId} but found {promPartnerIdAfterUpdate}");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(16)]
        public async Task B_PromotionPartnerNameUpdate_Success()
        {

            string[] partners = { "Virgin Atlantic Airways", "Air New Zealand", "Virgin Holidays", "Singapore Airlines", "Virgin Australia", "Air France" };

            int i = random.Next(partners.Length - 1);

            string promotionPartnerNameNew = partners[i];

            string strInputQueryForPromotionSearch = "Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.StartDate is not null and PR.EndDate is not null and PP.Name is not null order by NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            string promotionId = promotionDetails[0][0]?.ToString();

            string promotionName = promotionDetails[0][1]?.ToString();

            string promotionStartDate = DateTime.Parse(promotionDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDate = DateTime.Parse(promotionDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlag = promotionDetails[0][4]?.ToString();

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), promotionId,
                promotionName, promotionPartnerNameNew, promotionStartDate, promotionEndDate, promotionActiveFlag);

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionUpdateResult.data?.loyaltyUpdatePromotion?.ToString() == "Promotion details updated successfully",
                $"Expected the promotion Name Update to be successful for the {promotionId} but found {promotionUpdateResult}");

            int retry = 5;

            strInputQueryForPromotionSearch = $"Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PP.Name='{promotionPartnerNameNew}' and PR.RowId='{promotionId}' ";

            object[][] promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            while (promotionDetailsNew.Length == 0 && retry > 0)
            {
                await Task.Delay(1000);

                promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

                retry--;
            }

            string promotionNameAfterUpdate = promotionDetailsNew[0][1]?.ToString();

            string promotionStartDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlagAfterUpdate = promotionDetailsNew[0][4]?.ToString();

            string promPartnerNameAfterUpdate = promotionDetailsNew[0][6]?.ToString();

            Assert.True(promPartnerNameAfterUpdate.Equals(promotionPartnerNameNew),
                $"When Promotion PartnerName is Updated ,The Promotion Name is not as expected. Expected value {promotionName} but found {promotionNameAfterUpdate}");

            Assert.True(promotionNameAfterUpdate.Equals(promotionName),
                $"When Promotion PartnerName is Updated ,The Promotion Name is not as expected. Expected value {promotionName} but found {promotionNameAfterUpdate}");

            Assert.True(promotionStartDateAfterUpdate.Equals(promotionStartDate),
               $"When Promotion PartnerName is Updated ,The Promotion StartDate is not as expected. Expected value {promotionStartDate} but found {promotionStartDateAfterUpdate}");

            Assert.True(promotionEndDateAfterUpdate.Equals(promotionEndDate),
               $"When Promotion PartnerName is Updated ,The Promotion EndDate is not as expected. Expected value {promotionEndDate} but found {promotionEndDateAfterUpdate}");

            Assert.True(promotionActiveFlagAfterUpdate.Equals(promotionActiveFlag),
               $"When Promotion PartnerName is Updated ,The Promotion Active Flag is not as expected. Expected value {promotionActiveFlag} but found {promotionActiveFlagAfterUpdate}");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(17)]
        public async Task C_PromotionStartDateUpdate_Success()
        {

            string strInputQueryForPromotionSearch = "Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.StartDate is not null and PR.EndDate is not null and PP.Name is not null order by NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            string promotionId = promotionDetails[0][0]?.ToString();

            string promotionName = promotionDetails[0][1]?.ToString();

            string promotionEndDate = DateTime.Parse(promotionDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlag = promotionDetails[0][4]?.ToString();

            string promPartnerId = promotionDetails[0][5]?.ToString();

            string promPartnerName = promotionDetails[0][6]?.ToString();

            string promotionStartDateNew = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), promotionId,
                promotionName, promPartnerName, promotionStartDateNew, promotionEndDate, promotionActiveFlag);

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionUpdateResult.data?.loyaltyUpdatePromotion?.ToString() == "Promotion details updated successfully",
                $"Expected the promotion Name Update to be successful for the {promotionId} but found {promotionUpdateResult}");

            int retry = 5;

            strInputQueryForPromotionSearch = $"Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.StartDate='{promotionStartDateNew}' and PR.RowId='{promotionId}' ";

            object[][] promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            while (promotionDetailsNew.Length == 0 && retry > 0)
            {
                await Task.Delay(1000);

                promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

                retry--;
            }


            string promotionNameAfterUpdate = promotionDetailsNew[0][1]?.ToString();

            string promotionStartDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlagAfterUpdate = promotionDetailsNew[0][4]?.ToString();

            string promPartnerIdAfterUpdate = promotionDetailsNew[0][5]?.ToString();

            Assert.True(promotionNameAfterUpdate.Equals(promotionName),
                $"When Promotion StartDate is Updated ,The Promotion Name is not as expected. Expected value {promotionName} but found {promotionNameAfterUpdate}");

            Assert.True(promotionStartDateAfterUpdate.Equals(promotionStartDateNew),
               $"When Promotion StartDate is Updated ,The Promotion StartDate is not as expected. Expected value {promotionStartDateNew} but found {promotionStartDateAfterUpdate}");

            Assert.True(promotionEndDateAfterUpdate.Equals(promotionEndDate),
               $"When Promotion StartDate is Updated ,The Promotion EndDate is not as expected. Expected value {promotionEndDate} but found {promotionEndDateAfterUpdate}");

            Assert.True(promotionActiveFlagAfterUpdate.Equals(promotionActiveFlag),
               $"When Promotion StartDate is Updated ,The Promotion Active Flag is not as expected. Expected value {promotionActiveFlag} but found {promotionActiveFlagAfterUpdate}");

            Assert.True(promPartnerIdAfterUpdate.Equals(promPartnerId),
              $"When Promotion StartDate is Updated ,The Promotion Partner Id is not as expected. Expected value {promPartnerId} but found {promPartnerIdAfterUpdate}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(18)]
        public async Task D_PromotionEndDateUpdate_Success()
        {

            string strInputQueryForPromotionSearch = "Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.StartDate is not null and PR.EndDate is not null and PP.Name is not null order by NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            string promotionId = promotionDetails[0][0]?.ToString();

            string promotionName = promotionDetails[0][1]?.ToString();

            string promotionStartDate = DateTime.Parse(promotionDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlag = promotionDetails[0][4]?.ToString();

            string promPartnerId = promotionDetails[0][5]?.ToString();

            string promPartnerName = promotionDetails[0][6]?.ToString();

            string promotionEndDateNew = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), promotionId,
                promotionName, promPartnerName, promotionStartDate, promotionEndDateNew, promotionActiveFlag);

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionUpdateResult.data?.loyaltyUpdatePromotion?.ToString() == "Promotion details updated successfully",
                $"Expected the promotion Name Update to be successful for the {promotionId} but found {promotionUpdateResult}");

            int retry = 5;

            strInputQueryForPromotionSearch = $"Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.EndDate='{promotionEndDateNew}' and PR.RowId='{promotionId}' ";

            object[][] promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            while (promotionDetailsNew.Length == 0 && retry > 0)
            {
                await Task.Delay(1000);

                promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

                retry--;
            }


            string promotionNameAfterUpdate = promotionDetailsNew[0][1]?.ToString();

            string promotionStartDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlagAfterUpdate = promotionDetailsNew[0][4]?.ToString();

            string promPartnerIdAfterUpdate = promotionDetailsNew[0][5]?.ToString();

            Assert.True(promotionNameAfterUpdate.Equals(promotionName),
                $"When Promotion EndDate is Updated ,The Promotion Name is not as expected. Expected value {promotionName} but found {promotionNameAfterUpdate}");

            Assert.True(promotionStartDateAfterUpdate.Equals(promotionStartDate),
               $"When Promotion EndDate is Updated ,The Promotion StartDate is not as expected. Expected value {promotionStartDate} but found {promotionStartDateAfterUpdate}");

            Assert.True(promotionEndDateAfterUpdate.Equals(promotionEndDateNew),
               $"When Promotion EndDate is Updated ,The Promotion EndDate is not as expected. Expected value {promotionEndDateNew} but found {promotionEndDateAfterUpdate}");

            Assert.True(promotionActiveFlagAfterUpdate.Equals(promotionActiveFlag),
               $"When Promotion EndDate is Updated ,The Promotion Active Flag is not as expected. Expected value {promotionActiveFlag} but found {promotionActiveFlagAfterUpdate}");

            Assert.True(promPartnerIdAfterUpdate.Equals(promPartnerId),
              $"When Promotion EndDate is Updated ,The Promotion Partner Id is not as expected. Expected value {promPartnerId} but found {promPartnerIdAfterUpdate}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(19)]
        public async Task E_PromotionActiveFlagUpdate_Success()
        {

            string strInputQueryForPromotionSearch = "Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.StartDate is not null and PR.EndDate is not null and PP.Name is not null order by NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            string promotionId = promotionDetails[0][0]?.ToString();

            string promotionName = promotionDetails[0][1]?.ToString();

            string promotionStartDate = DateTime.Parse(promotionDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDate = DateTime.Parse(promotionDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlagNew = promotionDetails[0][4]?.ToString() == "Y" ? "N" : "Y";

            string promPartnerId = promotionDetails[0][5]?.ToString();

            string promPartnerName = promotionDetails[0][6]?.ToString();

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), promotionId,
                promotionName, promPartnerName, promotionStartDate, promotionEndDate, promotionActiveFlagNew);

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionUpdateResult.data?.loyaltyUpdatePromotion?.ToString() == "Promotion details updated successfully",
                $"Expected the promotion Name Update to be successful for the {promotionId} but found {promotionUpdateResult}");

            int retry = 5;

            strInputQueryForPromotionSearch = $"Select TOP 1 PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nWhere PR.Active='{promotionActiveFlagNew}' and PR.RowId='{promotionId}' ";

            object[][] promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            while (promotionDetailsNew.Length == 0 && retry > 0)
            {
                await Task.Delay(1000);

                promotionDetailsNew = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

                retry--;
            }


            string promotionNameAfterUpdate = promotionDetailsNew[0][1]?.ToString();

            string promotionStartDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDateAfterUpdate = DateTime.Parse(promotionDetailsNew[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionActiveFlagAfterUpdate = promotionDetailsNew[0][4]?.ToString();

            string promPartnerIdAfterUpdate = promotionDetailsNew[0][5]?.ToString();

            Assert.True(promotionNameAfterUpdate.Equals(promotionName),
                $"When Promotion Active Flag is Updated ,The Promotion Name is not as expected. Expected value {promotionName} but found {promotionNameAfterUpdate}");

            Assert.True(promotionStartDateAfterUpdate.Equals(promotionStartDate),
               $"When Promotion Active Flag is Updated ,The Promotion StartDate is not as expected. Expected value {promotionStartDate} but found {promotionStartDateAfterUpdate}");

            Assert.True(promotionEndDateAfterUpdate.Equals(promotionEndDate),
               $"When Promotion Active Flag is Updated ,The Promotion EndDate is not as expected. Expected value {promotionEndDate} but found {promotionEndDateAfterUpdate}");

            Assert.True(promotionActiveFlagAfterUpdate.Equals(promotionActiveFlagNew),
               $"When Promotion Active Flag is Updated ,The Promotion Active Flag is not as expected. Expected value {promotionActiveFlagNew} but found {promotionActiveFlagAfterUpdate}");

            Assert.True(promPartnerIdAfterUpdate.Equals(promPartnerId),
              $"When Promotion Active Flag is Updated ,The Promotion Partner Id is not as expected. Expected value {promPartnerId} but found {promPartnerIdAfterUpdate}");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(20)]
        public async Task G_PromotionUpdate_ShouldThrowMandatoryFieldMissingErrorWhenPromotionNameIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-2CMLL3S", "",
                "Virgin Atlantic Airways", "01-Jan-2000", "31-Dec-2050", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse), $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(21)]
        public async Task H_PromotionUpdate_ShouldThrowMandatoryFieldMissingErrorWhenPartnerNameIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-2CMLL3S",
                "TestABCD", "", "01-Jan-2000", "31-Dec-2050", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(22)]
        public async Task I_PromotionUpdate_ShouldThrowMandatoryFieldMissingErrorWhenStartDateIsNull()
        {
            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-2CMLL3S",
                "TestABCD", "Virgin Atlantic Airways", "", "31-Dec-2050", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(23)]
        public async Task J_PromotionUpdate_ShouldThrowMandatoryFieldMissingErrorWhenEndDateIsNull()
        {
            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-2CMLL3S",
                "TestABCD", "Virgin Atlantic Airways", "31-Dec-2050", "", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(24)]
        public async Task K_PromotionUpdate_ShouldThrowMandatoryFieldMissingErrorWhenActiveFlagIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-2CMLL3S", "TestABCD",
                "Virgin Atlantic Airways", "31-Dec-2010", "31-Dec-2050", "");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(25)]
        public async Task L_PromotionUpdate_ShouldThrowInvalidDateFormatErrorWhenStartDateFormatIsInvalid()
        {
            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-2CMLL3S", "TestPromotionDEV",
                    "Virgin Atlantic Airways", "13-12-2000", "31-Dec-2050", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid Date Formate.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(26)]
        public async Task M_PromotionUpdate_ShouldThrowInvalidDateFormatErrorWhenEndDateFormatIsInvalid()
        {

            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-2CMLL3S",
                "TestPromotionDEV", "Virgin Atlantic Airways", "12-12-2000", "13-12-2050", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid Date Formate.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(27)]
        public async Task N_PromotionUpdate_ShouldThrowInvalidPromotionIdErrorWhenPromotionIdIsInvalid()
        {
            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-7D85", "TestPromotionDEV",
                "Virgin Atlantic Airways", "12-12-2000", "12-12-2050", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid Promotion Id.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse),
                $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(28)]
        public async Task O_PromotionUpdate_ShouldThrowDuplicatePromotionNameErrorWhenPromotionNameIsAlreadyUsed()
        {
            HttpResponseMessage response = GraphQL.PromotionUpdate(tokenFixture.GetToken("graphqlv2"), "1-1GDZMY9",
                     "VS Tiers Red 1", "Virgin Atlantic Airways", "12-12-2000", "12-12-2050", "Y");

            dynamic promotionUpdateResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Already Used Promotion Name.";

            Assert.True(promotionUpdateResult.errors[0]?.extensions?.response?.ToString().Contains(expectedResponse), $"Text value for Promotion Update is not as expected. Expected {expectedResponse} but was {promotionUpdateResult.errors[0]?.extensions?.response?.ToString()}");

        }
    }
}
