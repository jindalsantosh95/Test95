using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class LoyaltyFlyingClubCustomerProfileTests : IClassFixture<TokenFixture>
    {
        private const int MaxSignInAttempts = 5;

        private readonly string env = Config.Environment;
        private readonly TokenFixture _tokenFixture;

        public LoyaltyFlyingClubCustomerProfileTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForConciergeGraphQlRestFromKeyVault("Rest");
        }

        [Theory]
        [InlineData("Red")]
        [InlineData("Gold")]
        [InlineData("Silver")]
        public async Task TC01_LoyaltyFlyingClubCustomerProfile_ReturnsCustomerProfileAndAccount(
            string tier)
        {
            string fcNumber = string.Empty;

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                Dictionary<string, string> loyaltyDetails = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tier: tier, points: 0);

                if (loyaltyDetails.TryGetValue("error", out string? error))
                {
                    Assert.Fail($"Test case failed: {error}");
                }

                if (!loyaltyDetails.TryGetValue("rowKey", out string? rowKey) ||
                    string.IsNullOrWhiteSpace(rowKey))
                {
                    Assert.Fail($"No loyalty account found for tier: {tier}");
                }

                fcNumber = rowKey;
                }, maxRetries: MaxSignInAttempts);

            
            JObject tokenResponse = JObject.Parse(await RestEndpoint.LoyaltyIssueFlyingClubApplicationToken(env, _tokenFixture.GetToken("Rest"), fcNumber).EnsureIsSuccessStatusCode().Content.ReadAsStringAsync());

            string accessToken =tokenResponse.RequireToken("accessToken").ToString();

            Assert.False(string.IsNullOrWhiteSpace(accessToken),$"Expected a valid access token for FC number {fcNumber}.");

            HttpResponseMessage response =RestEndpoint.LoyaltyFlyingClubCustomerProfile(env, accessToken).EnsureIsSuccessStatusCode();

            JObject profile = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Null(profile["errors"]);

            Assert.True(fcNumber.Is(profile.RequireToken("customer.loyaltyNumber").ToString()));

            Assert.False(string.IsNullOrWhiteSpace(profile.RequireToken("customer.customerId").ToString()));

            Assert.False(string.IsNullOrWhiteSpace(profile.RequireToken("customer.status").ToString()));

            Assert.False(string.IsNullOrWhiteSpace(profile.RequireToken("profile.brand").ToString()));

            Assert.NotEmpty(profile.RequireToken("profile.preferences"));

            Assert.False(string.IsNullOrWhiteSpace(profile.RequireToken("account.memberNumber").ToString()));

            Assert.False(string.IsNullOrWhiteSpace(profile.RequireToken("account.tierCode").ToString()));

            Assert.False(string.IsNullOrWhiteSpace(profile.RequireToken("account.memberType").ToString()));
        }
    }
}
