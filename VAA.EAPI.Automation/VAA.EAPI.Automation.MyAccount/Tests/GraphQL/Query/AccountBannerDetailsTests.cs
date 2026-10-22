using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Fixtures;
using VAA.EAPI.Automation.MyAccount.Models.GraphQL;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public sealed class AccountBannerDetailsTests : IClassFixture<StoredLoyaltyMemberAccountFixture>
    {
        private readonly string env = Config.Environment;
        private readonly StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture;

        public AccountBannerDetailsTests(StoredLoyaltyMemberAccountFixture loyaltyMemberAccountFixture)
        {
            this.loyaltyMemberAccountFixture = loyaltyMemberAccountFixture;
        }

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_AccountBannerDetails_ReturnsSupportedBannerData()
        {
            HttpResponseMessage response = GraphQL.AccountBannerDetails(env, loyaltyMemberAccountFixture.Token).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();

            AccountBannerDetailsResponse? parsedResponse = JsonConvert.DeserializeObject<AccountBannerDetailsResponse>(body);
            Assert.True(parsedResponse is not null, $"Expected a deserializable {nameof(AccountBannerDetailsResponse)} response for member '{loyaltyMemberAccountFixture.MemberNumber}', but found an invalid response.");
            AccountBannerDetailsResponse bannerResponse = parsedResponse ?? new AccountBannerDetailsResponse();

            Assert.True(bannerResponse.Data is not null, $"Expected data in the accountBannerDetails response for member '{loyaltyMemberAccountFixture.MemberNumber}', but found null.");
            AccountBannerDetailsResponseData data = bannerResponse.Data ?? new AccountBannerDetailsResponseData();

            Assert.True(data.AccountBannerDetails is not null, $"Expected accountBannerDetails for member '{loyaltyMemberAccountFixture.MemberNumber}', but found null.");
            AccountBannerDetails bannerDetails = data.AccountBannerDetails ?? new AccountBannerDetails();

            Assert.True(bannerDetails.MemberNumber is not null && loyaltyMemberAccountFixture.MemberNumber.Is(bannerDetails.MemberNumber), $"Expected banner details for member '{loyaltyMemberAccountFixture.MemberNumber}', but found '{bannerDetails.MemberNumber}'.");
            Assert.True(bannerDetails.Banners is not null, $"Expected a banner collection for member '{loyaltyMemberAccountFixture.MemberNumber}', but found null.");
            AccountBanner[] banners = bannerDetails.Banners ?? Array.Empty<AccountBanner>();
            Assert.NotEmpty(banners);

            // The resolver exposes only supported banner codes and derives this flag from the last-shown date.
            Assert.All(banners, banner =>
            {
                Assert.True(banner.BannerCode?.Is("VAC_TO_FC_UPGRADE") == true, $"Expected supported banner code 'VAC_TO_FC_UPGRADE', but found '{banner.BannerCode}'.");
                Assert.Equal(banner.LastShownDateTime.HasValue, banner.IsLastShown);
            });
        }
    }
}
