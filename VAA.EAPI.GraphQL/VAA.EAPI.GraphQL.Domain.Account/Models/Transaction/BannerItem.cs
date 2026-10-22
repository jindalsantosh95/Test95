using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class BannerItem
    {
        [JsonProperty("bannerCode")]
        public string BannerCode { get; set; }
    }
}
