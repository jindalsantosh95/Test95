using Newtonsoft.Json;
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
    public class LinkCampaignPromotionTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;

        public LinkCampaignPromotionTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(65)]
        public async Task A_CampaignPromotionLinkingandDelinking_Success()
        {

            string endDate = DateTime.UtcNow.AddDays(+5).ToString("yyyy-MM-dd");

            string strQueryForPromotionId = $"Select TOP 1 RowId from dbo.Promotions where RowId not in ( Select PromotionId from dbo.CampaignsPromotionsMapping) and Active='Y' and EndDate > '{endDate}' order by NEWID()";

            string strQueryForCampaignId = $"Select TOP 1 RowId from dbo.Campaigns where RowId not in ( Select CampaignId from dbo.CampaignsPromotionsMapping) and EndDate > '{endDate}' order by NEWID()";

            string promotionId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForPromotionId);

            string campaignId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForCampaignId);

            HttpResponseMessage response = GraphQL.CampaignPromotionLink(tokenFixture.GetToken("graphqlv2"), campaignId,
              promotionId, "Test@AutoReply.Com").EnsureIsSuccessStatusCode();

            string expectedResponse = "Campaign Promotion linked successfully";

            dynamic campaignPromotionLinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string strInputQueryForPromotionLinking = $"Select PromotionId from dbo.CampaignsPromotionsMapping where CampaignId='{campaignId}'";

            string promId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromotionLinking);

            Assert.True(promotionId.Equals(promId),
                        $"The Promotion linking is not working as expected. Expected {promotionId} but found {promId}");

            Assert.True(campaignPromotionLinkResult?.data?.loyaltyLinkCampaignPromotion?.ToString() == expectedResponse,
                $"Expected {expectedResponse} but was {campaignPromotionLinkResult?.ToString()}");

            HttpResponseMessage delinkCampaignPromotionResponse = GraphQL.DelinkCampaignPromotion(tokenFixture.GetToken("graphqlv2"),
                 campaignId, promotionId).EnsureIsSuccessStatusCode();

            dynamic delinkCampaignPromotionResult = JsonConvert.DeserializeObject<dynamic>(await delinkCampaignPromotionResponse.Content.ReadAsStringAsync());

            string expectedDelinkCampaignPromotionResponse = "Campaign Promotion delinked successfully";

            Assert.True(delinkCampaignPromotionResult.data?.loyaltyDelinkCampaignPromotion?.ToString() == expectedDelinkCampaignPromotionResponse,
                $"Text value for Campaign unassign is not as expected. Expected {expectedDelinkCampaignPromotionResponse} but was {delinkCampaignPromotionResult.ToString()}");

            promId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromotionLinking);

            Assert.True(string.IsNullOrEmpty(promId),
                        $"The Promotion Delinking is not working as expected. Expected null or empty but found {promId}");

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(66)]
        public async Task B_CampaignPromotionLinking_ShouldThrowMandatoryFieldMissingErrorWhenTheCampaignIdIsNull()
        {
            HttpResponseMessage response = GraphQL.CampaignPromotionLink(tokenFixture.GetToken("graphqlv2"), "", "1-2OHIEB",
                            "Autotest@noreply.com");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic campaignPromotionLinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(campaignPromotionLinkResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  but was {campaignPromotionLinkResult.ToString()}");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(67)]
        public async Task C_CampaignPromotionLinking_ShouldThrowMandatoryFieldMissingErrorWhenThePromotionIdIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignPromotionLink(tokenFixture.GetToken("graphqlv2"), "1-F8-203", "",
                "Autotest@noreply.com");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic campaignPromotionLinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(campaignPromotionLinkResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse} " +
                $" but was {campaignPromotionLinkResult.ToString()}");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(68)]
        public async Task D_CampaignPromotionLinking_ShouldThrowMandatoryFieldMissingErrorWhenTheCreatedByIsNull()
        {

            HttpResponseMessage response = GraphQL.CampaignPromotionLink(tokenFixture.GetToken("graphqlv2"), "1-F8-203",
                "1-2OHIEB", "");

            string expectedResponse = "One of the mandatory field is missing in the request.";

            dynamic campaignPromotionLinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(campaignPromotionLinkResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  " +
                $"but was {campaignPromotionLinkResult.ToString()}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(69)]
        public async Task E_CampaignPromotionLinking_ShouldThrowInvalidCampaignIdErrorWhenTheCampaignIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.CampaignPromotionLink(tokenFixture.GetToken("graphqlv2"), "1-GT2LU10",
                "1-2OHIEB", "test@autoreply.com");

            string expectedResponse = "Invalid Campaign Id.";

            dynamic campaignPromotionLinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(campaignPromotionLinkResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  " +
                $"but was {campaignPromotionLinkResult.ToString()}");
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(70)]
        public async Task F_CampaignPromotionLinking_ShouldThrowInvalidCampaignIdErrorWhenThePromotionIdIsInvalid()
        {

            HttpResponseMessage response = GraphQL.CampaignPromotionLink(tokenFixture.GetToken("graphqlv2"), "1-F8-203", "1-2OHIEBDF",
                "test@autoreply.com");

            string expectedResponse = "Invalid Promotion Id.";

            dynamic campaignPromotionLinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(campaignPromotionLinkResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  " +
                $"but was {campaignPromotionLinkResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(71)]
        public async Task G_CampaignPromotionLinking_ShouldThrowDuplicatePromotionLinkErrorWhenTheUserTriesToLinkAlreadyLinkedPromotionToACampaign()
        {

            HttpResponseMessage response = GraphQL.CampaignPromotionLink(tokenFixture.GetToken("graphqlv2"), "1-F8-203", "1-96CP51",
                "test@autoreply.com");

            string expectedResponse = "Already Linked Campaign and Promotion.";

            dynamic campaignPromotionLinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(campaignPromotionLinkResult.ToString().Contains(expectedResponse),
                $"Text value for note is not as expected. Expected {expectedResponse}  " +
                $"but was {campaignPromotionLinkResult.ToString()}");
        }

    }
}
