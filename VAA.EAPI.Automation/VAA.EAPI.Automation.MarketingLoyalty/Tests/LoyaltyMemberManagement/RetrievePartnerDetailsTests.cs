using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberManagement
{
    public class RetrievePartnerDetailsTests : IClassFixture<TokenFixture>
    {

        private readonly TokenFixture tokenFixture;

        public RetrievePartnerDetailsTests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        public async Task A_PartnerRetrieval_SuccessScenario()
        {

            HttpResponseMessage response = GraphQL.PartnerRetrieval(tokenFixture.GetToken("graphqlv2"));

            dynamic partnerRetrievalResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            int partnerCount = partnerRetrievalResult.data.loyaltyPartners.Count;

            string strQueryForPartnerProfileCount = $"select count(RowID) from dbo.PartnerProfile";

            int partnerProfileCountFromDB = (int)AzureReferenceDBHelper.RefDbQueryResponse(strQueryForPartnerProfileCount);

            Assert.True(partnerCount == partnerProfileCountFromDB,
              $"The Partner Retrieval count is not as expected. Expected {partnerProfileCountFromDB} " +
              $" but was {partnerCount}");
        }
    }
}
