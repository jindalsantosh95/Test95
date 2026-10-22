using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class Banner
    {
        [JsonProperty("banner")]
        public BannerData BannerData { get; set; }

    }
}
