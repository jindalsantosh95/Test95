using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public sealed class AccountTenureDetailsTests
    {
        private readonly string env = Config.Environment;

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_AccountTenureDetails_ReturnsStoredHighFiveActiveYears()
        {
            Dictionary<string, string> highFiveAccount = RetrieveFromStorageTable.RetrieveLoyaltyHighFiveAccountFromStorageTable();
            Assert.False(highFiveAccount.ContainsKey("error"), highFiveAccount.GetValueOrDefault("error"));

            string memberNumber = highFiveAccount["memberNumber"];
            string[] expectedActiveYears = highFiveAccount["activeYears"]
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(year => int.Parse(year))
                .ToArray();
            Assert.NotEmpty(expectedActiveYears);

            // High Five test data supports direct sign-in with its stored Flying Club member number.
            string legacyPassword = Vault.Default.GetSecretValue("MYA-LegacyPassword");
            string idToken = await EntraTokenHelper.GetIdTokenWithPasswordFallback(
                env,
                string.Empty,
                memberNumber,
                Application.LoginPassword,
                legacyPassword);
            HttpResponseMessage response = GraphQL.AccountTenureDetails(env, idToken).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(body);
            JToken documents = result.RequireToken("data.accountTenureDetails.memberCustomAttributesDocuments");
            Assert.Equal(JTokenType.Array, documents.Type);
            JToken document = Assert.Single(documents);

            JToken activityYears = document.RequireToken("memberLastFiveVSActivityYear");
            Assert.Equal(JTokenType.Array, activityYears.Type);

            string[] actualActiveYears = activityYears
                .Select(activityYear => activityYear.RequireToken("activityYear").ToString())
                .ToArray();
            Assert.Equal(expectedActiveYears, actualActiveYears);

            Assert.True(int.TryParse(document.RequireToken("activeYearCounterForTenureBenefits").ToString(), out int activeYearCounter), "activeYearCounterForTenureBenefits should be numeric.");
            Assert.Equal(expectedActiveYears.Length, activeYearCounter);

            bool isCurrentYearBenefitGiven = activityYears.Any(activityYear =>
                activityYear.RequireToken("activityYear").ToString() == DateTime.UtcNow.Year.ToString() &&
                activityYear.RequireToken("benefitGiven").ToString().Is("true"));
            Assert.Equal(isCurrentYearBenefitGiven, document.RequireToken("isAnActiveYear").Value<bool>());
        }

    }
}

