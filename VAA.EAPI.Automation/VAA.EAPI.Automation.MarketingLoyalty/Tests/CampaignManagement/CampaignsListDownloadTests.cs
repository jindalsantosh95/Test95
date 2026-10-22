using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.CampaignManagement
{
    public class CampaignsListDownloadTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public CampaignsListDownloadTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact]
        public async Task CampaignsListDownload_Success()
        {
            #region GraphQL Endpoint Call and Validation

            HttpResponseMessage response = GraphQL.DownloadCampaignsList(_tokenFixture.GetToken("graphqlv2")).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string currentDate = DateTime.UtcNow.ToString("MM-dd-yyyy").Replace("-", "");

            string expectedUrl = $"campaigns/Campaigns_{currentDate}.xlsx";

            Assert.True(result.ToString().Contains(expectedUrl),
                $"Expected the Total campaign list url to be  {expectedUrl} but found {result.ToString()}.");

            #endregion
        }

    }
}
