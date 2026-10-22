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
    public sealed class AccountManageAccountTests : IClassFixture<StoredLoyaltyMemberAccountFixture>
    {
        private readonly string env = Config.Environment;
        private readonly StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture;

        public AccountManageAccountTests(StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture)
        {
            this.loyaltyMemberAccountFixture = loyaltyMemberAccountFixture;
        }

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_AccountManageAccount_ReturnsMemberProfile()
        {
            HttpResponseMessage response = GraphQL.AccountManageAccount(env, loyaltyMemberAccountFixture.Token).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(body);
            JToken manageAccount = result.RequireToken("data.accountManageAccount");

            Assert.False(manageAccount.RequireToken("memberType").ToString().IsNullOrWhiteSpace(), "Expected memberType to be returned.");
            Assert.False(manageAccount.RequireToken("firstName").ToString().IsNullOrWhiteSpace(), "Expected firstName to be returned.");
            Assert.False(manageAccount.RequireToken("lastName").ToString().IsNullOrWhiteSpace(), "Expected lastName to be returned.");
            Assert.False(manageAccount.RequireToken("email").ToString().IsNullOrWhiteSpace(), "Expected email to be returned.");
            Assert.Equal(JTokenType.Boolean, manageAccount.RequireToken("isVAAMarketingPreferred").Type);

            JArray addresses = Assert.IsType<JArray>(manageAccount.RequireToken("address"));
            Assert.NotEmpty(addresses);
            JObject primaryAddress = Assert.IsType<JObject>(addresses.First);
            Assert.False(primaryAddress.RequireToken("country").ToString().IsNullOrWhiteSpace(), "Expected the primary address country to be returned.");
            Assert.False(primaryAddress.RequireToken("postalCode").ToString().IsNullOrWhiteSpace(), "Expected the primary address postal code to be returned.");
        }

    }
}

