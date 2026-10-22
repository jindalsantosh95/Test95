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
    public class CampaignTypeRetrieveTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        public CampaignTypeRetrieveTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(40)]
        public async Task A_RetrieveCampaignTypeSuccess()
        {
            HttpResponseMessage response = GraphQL.RetrieveCampaignType(tokenFixture.GetToken("graphqlv2")).EnsureIsSuccessStatusCode();

            dynamic retrieveCampaignTypeResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string[] expectedCampaignTypeList = {"Other", "Web Announcements", "Accrual", "Fast Track to Gold",
                                    "Flying Club Communication(CMD)", "Silver After First Flight",
                                    "Silver After 1st Flight + FTG", "Redemption", "Swinger",
                                    "Straight to Silver", "Straight to Gold", "Straight to Silver with FTG"};

            foreach (string campaignType in expectedCampaignTypeList)
            {
                Assert.True(retrieveCampaignTypeResult.ToString().Contains(campaignType),
                    $"The RetrieveCampaignType Response does not contain the campaign type {campaignType}," +
                    $" which is not as expected. ");
            }

        }
    }
}
