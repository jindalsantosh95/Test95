using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.PromotionManagement
{
    [Collection("PromotionManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class PromotionSearchAndRetrieveTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;

        public PromotionSearchAndRetrieveTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task A_PromotionSearchWithPromotionId_Success()
        {

            string strInputQueryForPromotionId = "Select Top 1 RowId from dbo.Promotions order by NEWID()";

            string promotionId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromotionId);

            HttpResponseMessage response = GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), promotionId,
                "", "", "", "", "", "", "").EnsureIsSuccessStatusCode();

            dynamic promotionSearchAndRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionSearchAndRetrieveResult.errors is null, $"Expected the details of the Promotion Id {promotionId} but found {promotionSearchAndRetrieveResult}");

            IEnumerable<PromotionSearchResponse> promotionSearchResponses = JsonConvert.DeserializeObject<PromotionSearchResponse[]>(promotionSearchAndRetrieveResult.data.loyaltyPromotionSearch.ToString());

            string strInputQueryForPromotionSearch = $"Select PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name ,\r\nCASE WHEN CP.PromotionId is null THEN 'N'\r\nElse 'Y'\r\nEnd as campaignLinked\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nleft join dbo.CampaignsPromotionsMapping CP on PR.RowId=CP.PromotionId \r\nwhere PR.RowId='{promotionId}'";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            string expectedPromotionId = promotionDetails[0][0]?.ToString();

            string expectedPromotionName = promotionDetails[0][1]?.ToString();

            string expectedPromotionStartDate = DateTime.Parse(promotionDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string expectedPromotionEndDate = DateTime.Parse(promotionDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string expectedPromotionActiveFlag = promotionDetails[0][4]?.ToString();

            string expectedPromPartnerId = promotionDetails[0][5]?.ToString();

            string expectedPromPartnerName = promotionDetails[0][6]?.ToString();

            string expectedCampaignLinkedFlag = promotionDetails[0][7]?.ToString();

            Assert.True(promotionSearchResponses.Count() == 1,
                $"Expected only one response object for promotion Search using PromotionId {promotionId} but found {promotionSearchResponses.Count()}");

            PromotionSearchResponse promotionSearchResponse = promotionSearchResponses.First();

            string actualPromotionStartDate = promotionSearchResponse.startDate.Value.ToString("dd-MMM-yyyy");

            string actualPromotionEndDate = promotionSearchResponse.endDate.Value.ToString("dd-MMM-yyyy");

            Assert.True(promotionSearchResponse.promotionId == expectedPromotionId,
                $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the PromotionId in the response to be {expectedPromotionId} but found {promotionSearchResponse.promotionId}");

            Assert.True(promotionSearchResponse.promotionName == expectedPromotionName,
                $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the PromotionName in the response to be {expectedPromotionName} but found {promotionSearchResponse.promotionId}");

            Assert.True(promotionSearchResponse.active == expectedPromotionActiveFlag,
                $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the Promotion Active Flag in the response to be {expectedPromotionActiveFlag} but found {promotionSearchResponse.active}");

            Assert.True(promotionSearchResponse.partnerId == expectedPromPartnerId,
                $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the Promotion Partner Id in the response to be {expectedPromPartnerId} but found {promotionSearchResponse.partnerId}");

            Assert.True(promotionSearchResponse.partnerName == expectedPromPartnerName,
                $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the Promotion PartnerName in the response to be {expectedPromPartnerName} but found {promotionSearchResponse.partnerName}");

            Assert.True(promotionSearchResponse.campaignLinked == expectedCampaignLinkedFlag,
                $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the Promotion CampaignLinked Flag in the response to be {expectedCampaignLinkedFlag} but found {promotionSearchResponse.campaignLinked}");

            Assert.True(promotionSearchResponse.promotionName == expectedPromotionName,
                $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the PromotionName in the response to be {expectedPromotionName} but found {promotionSearchResponse.promotionId}");

            Assert.True(actualPromotionStartDate == expectedPromotionStartDate,
               $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the Promotion StartDate in the response to be {expectedPromotionStartDate} but found {actualPromotionStartDate}");

            Assert.True(actualPromotionEndDate == expectedPromotionEndDate,
               $"For the Promotion Search using the Promotion Id {promotionId} ,Expected the Promotion EndDate in the response to be {expectedPromotionEndDate} but found {actualPromotionEndDate}");


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(10)]
        public async Task B_PromotionSearchWithPromotionName_Success()
        {
            string strInputQueryForPromotionName = "Select Top 1 Name from dbo.Promotions order by NEWID()";

            string promotionName = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromotionName);

            HttpResponseMessage response = GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), "",
                promotionName, "", "", "", "", "", "").EnsureIsSuccessStatusCode();

            dynamic promotionSearchAndRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionSearchAndRetrieveResult.errors is null,
                $"Expected the details of the Promotion Name {promotionName} but found {promotionSearchAndRetrieveResult}");

            IEnumerable<PromotionSearchResponse> promotionSearchResponses = JsonConvert.DeserializeObject<PromotionSearchResponse[]>(promotionSearchAndRetrieveResult.data.loyaltyPromotionSearch.ToString());

            string strInputQueryForPromotionSearch = $"Select PR.RowId,PR.Name,PR.StartDate,PR.EndDate,PR.Active,PP.RowId,PP.Name ,\r\nCASE WHEN CP.PromotionId is null THEN 'N'\r\nElse 'Y'\r\nEnd as campaignLinked\r\nfrom dbo.Promotions PR \r\nleft join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId \r\nleft join dbo.CampaignsPromotionsMapping CP on PR.RowId=CP.PromotionId \r\nwhere PR.Name='{promotionName}'";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(strInputQueryForPromotionSearch);

            string expectedPromotionId = promotionDetails[0][0]?.ToString();

            string expectedPromotionName = promotionDetails[0][1]?.ToString();

            string expectedPromotionStartDate = DateTime.Parse(promotionDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string expectedPromotionEndDate = DateTime.Parse(promotionDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string expectedPromotionActiveFlag = promotionDetails[0][4]?.ToString();

            string expectedPromPartnerId = promotionDetails[0][5]?.ToString();

            string expectedPromPartnerName = promotionDetails[0][6]?.ToString();

            string expectedCampaignLinkedFlag = promotionDetails[0][7]?.ToString();

            Assert.True(promotionSearchResponses.Count() == 1,
                $"Expected only one response object for promotion Search using PromotionName {promotionName} but found {promotionSearchResponses.Count()}");

            PromotionSearchResponse promotionSearchResponse = promotionSearchResponses.First();

            string actualPromotionStartDate = promotionSearchResponse.startDate.Value.ToString("dd-MMM-yyyy");

            string actualPromotionEndDate = promotionSearchResponse.endDate.Value.ToString("dd-MMM-yyyy");

            Assert.True(promotionSearchResponse.promotionId == expectedPromotionId,
                $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the PromotionId in the response to be {expectedPromotionId} but found {promotionSearchResponse.promotionId}");

            Assert.True(promotionSearchResponse.promotionName == expectedPromotionName,
                $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the PromotionName in the response to be {expectedPromotionName} but found {promotionSearchResponse.promotionId}");

            Assert.True(promotionSearchResponse.active == expectedPromotionActiveFlag,
                $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the Promotion Active Flag in the response to be {expectedPromotionActiveFlag} but found {promotionSearchResponse.active}");

            Assert.True(promotionSearchResponse.partnerId == expectedPromPartnerId,
                $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the Promotion Partner Id in the response to be {expectedPromPartnerId} but found {promotionSearchResponse.partnerId}");

            Assert.True(promotionSearchResponse.partnerName == expectedPromPartnerName,
                $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the Promotion PartnerName in the response to be {expectedPromPartnerName} but found {promotionSearchResponse.partnerName}");

            Assert.True(promotionSearchResponse.campaignLinked == expectedCampaignLinkedFlag,
                $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the Promotion CampaignLinked Flag in the response to be {expectedCampaignLinkedFlag} but found {promotionSearchResponse.campaignLinked}");

            Assert.True(promotionSearchResponse.promotionName == expectedPromotionName,
                $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the PromotionName in the response to be {expectedPromotionName} but found {promotionSearchResponse.promotionId}");

            Assert.True(actualPromotionStartDate == expectedPromotionStartDate,
               $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the Promotion StartDate in the response to be {expectedPromotionStartDate} but found {actualPromotionStartDate}");

            Assert.True(actualPromotionEndDate == expectedPromotionEndDate,
               $"For the Promotion Search using the Promotion Name {promotionName} ,Expected the Promotion EndDate in the response to be {expectedPromotionEndDate} but found {actualPromotionEndDate}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(11)]
        [InlineData("StartDateFrom")]
        [InlineData("StartDateTo")]
        public async Task C_PromotionSearchWithStartDate_Success(string scenario)
        {

            string strInputQueryForStartDate = "Select Top 1 StartDate from dbo.Promotions order by NEWID()";

            DateTime expectedPromotionStartDate = ((DateTime)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForStartDate)).Date;

            string startDate = expectedPromotionStartDate.ToString("dd-MMM-yyyy");

            HttpResponseMessage response = scenario == "StartDateFrom"
                ? GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), "",
                    "", startDate, "", "", "", "", "").EnsureIsSuccessStatusCode()
                : GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), "",
               "", "", "", startDate, "", "", "").EnsureIsSuccessStatusCode();

            dynamic promotionSearchAndRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionSearchAndRetrieveResult.errors is null, $"Expected the details of the Promotions when searched with {scenario} {startDate} but found {promotionSearchAndRetrieveResult}");

            IEnumerable<PromotionSearchResponse> promotionSearchResponses = JsonConvert.DeserializeObject<PromotionSearchResponse[]>(promotionSearchAndRetrieveResult.data.loyaltyPromotionSearch.ToString());

            string condition = scenario.Equals("StartDateFrom") ? "> =" : "< =";

            string strInputQueryForPromotionSearch = $"Select count(*) as count from dbo.Promotions where startDate {condition} '{startDate}'";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromotionSearch);

            Assert.True(promotionSearchResponses.Count() == count,
                $"Expected the total promotion list count for promotion Search using {scenario} to be {count} but found {promotionSearchResponses.Count()}");

            foreach (PromotionSearchResponse searchResponse in promotionSearchResponses)
            {

                DateTime actualPromotionStartDate = searchResponse.startDate.Value.Date;

                if (scenario.Equals("StartDateFrom"))
                    Assert.True(actualPromotionStartDate >= expectedPromotionStartDate,
                        $"For the Promotion Search using the Promotion  {scenario} ,Expected the Promotion StartDate to be {condition} {expectedPromotionStartDate} but found {actualPromotionStartDate}");

                else
                    Assert.True(actualPromotionStartDate <= expectedPromotionStartDate,
                        $"For the Promotion Search using the Promotion  {scenario} ,Expected the Promotion StartDate to be {condition} {expectedPromotionStartDate} but found {actualPromotionStartDate}");

            }
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(12)]
        [InlineData("EndDateFrom")]
        [InlineData("EndDateTo")]
        public async Task D_PromotionSearchWithEndDate_Success(string scenario)
        {

            string strInputQueryForEndDate = "Select Top 1 EndDate from dbo.Promotions order by NEWID()";

            DateTime expectedPromotionEndDate = ((DateTime)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForEndDate)).Date;

            string endDate = expectedPromotionEndDate.ToString("dd-MMM-yyyy");

            HttpResponseMessage response = scenario == "EndDateFrom"
                ? GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), "",
                    "", "", endDate, "", "", "", "").EnsureIsSuccessStatusCode()
                : GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), "",
               "", "", "", "", endDate, "", "").EnsureIsSuccessStatusCode();

            dynamic promotionSearchAndRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionSearchAndRetrieveResult.errors is null, $"Expected the details of the Promotions when searched with {scenario} {endDate} but found {promotionSearchAndRetrieveResult}");

            IEnumerable<PromotionSearchResponse> promotionSearchResponses = JsonConvert.DeserializeObject<PromotionSearchResponse[]>(promotionSearchAndRetrieveResult.data.loyaltyPromotionSearch.ToString());

            string condition = scenario.Equals("EndDateFrom") ? "> =" : "< =";

            string strInputQueryForPromotionSearch = $"Select count(*) as count from dbo.Promotions where endDate {condition} '{endDate}'";

            int count = (int)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromotionSearch);

            Assert.True(promotionSearchResponses.Count() == count,
                $"Expected the total promotion list count for promotion Search using {scenario} to be {count} but found {promotionSearchResponses.Count()}");

            foreach (PromotionSearchResponse searchResponse in promotionSearchResponses)
            {

                DateTime actualPromotionEndDate = searchResponse.endDate.Value.Date;

                if (scenario.Equals("EndDateFrom"))
                    Assert.True(actualPromotionEndDate >= expectedPromotionEndDate,
                        $"For the Promotion Search using the Promotion  {scenario} ,Expected the Promotion EndDate to be {condition} {expectedPromotionEndDate} but found {actualPromotionEndDate}");

                else
                    Assert.True(actualPromotionEndDate <= expectedPromotionEndDate,
                        $"For the Promotion Search using the Promotion  {scenario} ,Expected the Promotion EndDate to be {condition} {expectedPromotionEndDate} but found {actualPromotionEndDate}");

            }
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(13)]
        public async Task E_PromotionSearchWithPartnerName_Success()
        {

            string strInputQueryForPromoPartnerName = "Select TOP 1 PP.Name from dbo.Promotions PR left join dbo.PartnerProfile PP on PR.PartnerId=PP.RowId order by NEWID()";

            string partnerName = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromoPartnerName);

            HttpResponseMessage response = GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), "", "", "", "", "",
                "", partnerName, "").EnsureIsSuccessStatusCode();

            dynamic promotionSearchAndRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionSearchAndRetrieveResult.errors is null,
                $"Expected the details of the Promotions when searched with partnerName : {partnerName} but found {promotionSearchAndRetrieveResult}");

            IEnumerable<PromotionSearchResponse> promotionSearchResponses = JsonConvert.DeserializeObject<PromotionSearchResponse[]>(promotionSearchAndRetrieveResult.data.loyaltyPromotionSearch.ToString());

            foreach (PromotionSearchResponse searchResponse in promotionSearchResponses)
            {

                string actualPromotionPartnerName = searchResponse.partnerName;

                Assert.True(actualPromotionPartnerName == partnerName,
                $"For the Promotion Search using the Promotion PartnerName: {partnerName}, Expected the Promotion PartnerName to be {partnerName}  but found {actualPromotionPartnerName}");

            }
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(14)]
        [InlineData("Y")]
        [InlineData("N")]
        public async Task F_PromotionSearchWithAllActivePromotion_Success(string activeFlag)
        {

            HttpResponseMessage response = GraphQL.PromotionSearchAndRetrieve(tokenFixture.GetToken("graphqlv2"), "", "", "", "", "",
                "", "", activeFlag).EnsureIsSuccessStatusCode();

            dynamic promotionSearchAndRetrieveResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(promotionSearchAndRetrieveResult.errors is null,
                $"Expected the details of the Promotions when searched with ActiveFlag : {activeFlag} but found {promotionSearchAndRetrieveResult}");

            IEnumerable<PromotionSearchResponse> promotionSearchResponses = JsonConvert.DeserializeObject<PromotionSearchResponse[]>(promotionSearchAndRetrieveResult.data.loyaltyPromotionSearch.ToString());

            foreach (PromotionSearchResponse searchResponse in promotionSearchResponses)
            {
                string actualPromotionActiveFlag = searchResponse.active;

                Assert.True(actualPromotionActiveFlag == activeFlag,
                $"For the Promotion Search using the Promotion Active Flag: {activeFlag}, Expected the Promotion Active Flag to be {activeFlag}  but found {actualPromotionActiveFlag}");

            }
        }

    }
}
