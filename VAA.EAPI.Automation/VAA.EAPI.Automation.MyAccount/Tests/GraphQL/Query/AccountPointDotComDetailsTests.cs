using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Fixtures;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    public sealed class AccountPointDotComDetailsTests : IClassFixture<StoredLoyaltyMemberAccountFixture>
    {
        private readonly string env = Config.Environment;
        private readonly StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture;

        public AccountPointDotComDetailsTests(StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture)
        {
            this.loyaltyMemberAccountFixture = loyaltyMemberAccountFixture;
        }

        [Theory]
        [InlineData("Transfer")]
        [InlineData("Gift")]
        [InlineData("Buy")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.SharedServicesSignOff)]
    [Trait("SharedService", "SecurityMS")]
    [Trait("SharedServiceConsumer", "MyAccount")]
    [Trait("TestType", "Contract")]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
        public async Task TC01_AccountPointDotComDetails_ReturnsData(string operation)
        {
            HttpResponseMessage response = GraphQL.AccountPointDotComDetails(env, loyaltyMemberAccountFixture.Token, operation).EnsureGQLResponseSuccess();

            string body = await response.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(body);

            string pointDetails = result.RequireToken("data.accountPointDotComDetails").ToString();

            Assert.False(pointDetails.IsNullOrEmpty(), $"Expected a non-empty Point.com token for '{operation}'.");
        }

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC02_AccountPointDotComDetails_RejectsUnsupportedOperation()
        {
            HttpResponseMessage response = GraphQL.AccountPointDotComDetails(env, loyaltyMemberAccountFixture.Token, "Unsupported");
            JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.True("G057".Is(responseBody.RequireToken("errors[0].extensions.code").ToString()), "Expected the Point.com error code for an unsupported operation.");
        }

    }
}

