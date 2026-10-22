using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Models.GraphQL
{
    public sealed class AccountBannerDetailsResponse
    {
        [JsonProperty("data")]
        public AccountBannerDetailsResponseData? Data { get; set; }
    }

    public sealed class AccountBannerDetailsResponseData
    {
        [JsonProperty("accountBannerDetails")]
        public AccountBannerDetails? AccountBannerDetails { get; set; }
    }

    public sealed class AccountBannerDetails
    {
        [JsonProperty("FCNumber")]
        public string? MemberNumber { get; set; }

        [JsonProperty("Banners")]
        public AccountBanner[]? Banners { get; set; }
    }

    public sealed class AccountBanner
    {
        [JsonProperty("bannerCode")]
        public string? BannerCode { get; set; }

        [JsonProperty("lastShownDateTime")]
        public DateTime? LastShownDateTime { get; set; }

        [JsonProperty("isLastShown")]
        public bool IsLastShown { get; set; }
    }
}
