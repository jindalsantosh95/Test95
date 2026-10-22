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
    public class LoyaltyPromotionSearchCampaignTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;
        public LoyaltyPromotionSearchCampaignTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(72)]
        public async Task A_CampaignSearchWithPromotionId_SuccessScenario()
        {

            string strQueryForPromotionId = "Select TOP 1 PromotionId from dbo.CampaignsPromotionsMapping order by NEWID()";

            string promotionId = (string)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForPromotionId);

            HttpResponseMessage response = GraphQL.PromotionSearchCampaign(tokenFixture.GetToken("graphqlv2"), promotionId);

            dynamic promotionSearchCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            int campaignCount = promotionSearchCampaignResult.data.loyaltyPromotionCampaignSearch.Count;

            string strQueryForCampaignCount = $"select count(RowId) from [dbo].[CampaignsPromotionsMapping] where PromotionId = '{promotionId}'";

            int campaignCountFromDB = (int)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForCampaignCount);

            Assert.True(campaignCount == campaignCountFromDB,
              $"The Campaign count for given promotion id is not as expected. Expected {campaignCountFromDB} " +
              $" but was {campaignCount}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(73)]
        public async Task B_CampaignSearchWithPromotionId_ShouldThrowMandatoryFieldErrorWhenPromotionIdIsNull()
        {

            HttpResponseMessage response = GraphQL.PromotionSearchCampaign(tokenFixture.GetToken("graphqlv2"), "");

            dynamic promotionSearchCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(promotionSearchCampaignResult.ToString().Contains(expectedResponse),
                $"Text value for Promotion Search Campaign is not as expected. Expected {expectedResponse}" +
                $" but was {promotionSearchCampaignResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(74)]
        public async Task C_CampaignSearchWithPromotionId_ShouldThrowInvalidFieldErrorWhenPromotionIdIsIncorrect()
        {

            HttpResponseMessage response = GraphQL.PromotionSearchCampaign(tokenFixture.GetToken("graphqlv2"), "1-2OHI");

            dynamic promotionSearchCampaignResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "No Match Found.";

            Assert.True(promotionSearchCampaignResult.ToString().Contains(expectedResponse),
                "Text value for Promotion Search Campaign is not as expected." +
                $" Expected {expectedResponse} but was {promotionSearchCampaignResult.ToString()}");

        }
    }
}
