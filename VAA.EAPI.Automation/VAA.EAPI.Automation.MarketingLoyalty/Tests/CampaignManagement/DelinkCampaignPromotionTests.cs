using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CampaignManagement
{

    [Collection("CampaignManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class DelinkCampaignPromotionTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;
        public DelinkCampaignPromotionTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(61)]
        public async Task A_DelinkCampaignPromotion_ShouldThrowMandatoryFieldMissingErrorWhenCampaignIdIsNull()
        {

            HttpResponseMessage response = GraphQL.DelinkCampaignPromotion(tokenFixture.GetToken("graphqlv2"), "", "1-2OHIEB");

            dynamic delinkCampaignPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(delinkCampaignPromotionResult.ToString().Contains(expectedResponse),
                $"Text value for Campaign unassign is not as expected. Expected {expectedResponse} " +
                $"but was {delinkCampaignPromotionResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(62)]
        public async Task B_DelinkCampaignPromotion_ShouldThrowMandatoryFieldMissingErrorWhenPromotionIdIsNull()
        {

            HttpResponseMessage response = GraphQL.DelinkCampaignPromotion(tokenFixture.GetToken("graphqlv2"), "1-3W-607", "");

            dynamic delinkCampaignPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "One of the mandatory field is missing in the request.";

            Assert.True(delinkCampaignPromotionResult.ToString().Contains(expectedResponse),
                $"Text value for Campaign unassign is not as expected. Expected {expectedResponse}" +
                $" but was {delinkCampaignPromotionResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(63)]
        public async Task C_DelinkCampaignPromotion_ShouldThrowInvalidFieldErrorWhenCampaignIdIsIncorrect()
        {

            HttpResponseMessage response = GraphQL.DelinkCampaignPromotion(tokenFixture.GetToken("graphqlv2"), "1-3W-609", "1-2OHIEB");

            dynamic delinkCampaignPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid Campaign Id.";

            Assert.True(delinkCampaignPromotionResult.ToString().Contains(expectedResponse),
                $"Text value for Campaign unassign is not as expected. Expected {expectedResponse}" +
                $" but was {delinkCampaignPromotionResult.ToString()}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(64)]
        public async Task D_DelinkCampaignPromotion_ShouldThrowInvalidFieldErrorWhenPromotionIdIsIncorrect()
        {

            HttpResponseMessage response = GraphQL.DelinkCampaignPromotion(tokenFixture.GetToken("graphqlv2"), "1-3W-607", "1-2OHI");

            dynamic delinkCampaignPromotionResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Invalid Promotion Id.";

            Assert.True(delinkCampaignPromotionResult.ToString().Contains(expectedResponse),
                $"Text value for Delink Campaign Promotion is not as expected." +
                $" Expected {expectedResponse} but was {delinkCampaignPromotionResult.ToString()}");

        }
    }
}
