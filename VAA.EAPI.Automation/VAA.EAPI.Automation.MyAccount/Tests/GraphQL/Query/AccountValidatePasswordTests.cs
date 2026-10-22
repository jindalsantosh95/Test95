using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    public sealed class AccountValidatePasswordTests
    {
        private readonly string env = Config.Environment;

        [Theory]
        [InlineData("Virgin123", true)]
        [InlineData("VirginAtlantic", false)]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_AccountValidatePassword_ReturnsExpectedPolicyResult(string password, bool expectedIsValid)
        {
            HttpResponseMessage response = GraphQL.AccountValidatePassword(env, password).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(body);
            JArray policies = Assert.IsType<JArray>(result.RequireToken("data.accountValidatePassword"));

            Assert.NotEmpty(policies);

            // Every policy must identify the rule and state whether the supplied password meets it.
            foreach (JToken policy in policies)
            {
                Assert.False(policy.RequireToken("policyId").ToString().IsNullOrWhiteSpace(), "Expected each password policy to have an ID.");
                Assert.False(policy.RequireToken("policyDescription").ToString().IsNullOrWhiteSpace(), "Expected each password policy to have a description.");
                Assert.Equal(JTokenType.Boolean, policy.RequireToken("isValid").Type);
            }

            bool isValid = policies.All(policy => policy.RequireToken("isValid").Value<bool>());
            Assert.Equal(expectedIsValid, isValid);
        }
    }
}

