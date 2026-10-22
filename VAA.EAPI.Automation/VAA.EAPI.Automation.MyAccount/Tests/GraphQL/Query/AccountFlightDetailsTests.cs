using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Fixtures;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public sealed class AccountFlightDetailsTests : IClassFixture<StoredLoyaltyMemberAccountFixture>
    {
        private readonly string env = Config.Environment;
        private readonly StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture;

        public AccountFlightDetailsTests(StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture)
        {
            this.loyaltyMemberAccountFixture = loyaltyMemberAccountFixture;
        }

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_AccountFlightDetails_ReturnsValidResponse()
        {
            HttpResponseMessage response = GraphQL.AccountFlightDetails(env, loyaltyMemberAccountFixture.Token).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(body), "Expected a non-empty accountFlightDetails response body.");

            JObject responseBody = JObject.Parse(body);
            JToken data = responseBody.RequireToken("data");
            JToken? flightDetails = data.SelectToken("accountFlightDetails");
            if (flightDetails is null)
            {
                Assert.Fail("Expected data.accountFlightDetails to be present in the GraphQL response.");
                return;
            }

            // Flight data is not consistently available in the shared environment, so a null result is a valid response.
            if (flightDetails.Type == JTokenType.Null)
            {
                return;
            }

            Assert.Equal(JTokenType.Object, flightDetails.Type);
            JToken flightSegmentDetails = flightDetails.RequireToken("flightSegmentDetails");
            Assert.Equal(JTokenType.Array, flightSegmentDetails.Type);
        }

    }
}
