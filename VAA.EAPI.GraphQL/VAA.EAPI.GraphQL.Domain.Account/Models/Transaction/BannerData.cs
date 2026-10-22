using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class BannerData
    {
        public string CustomerID { get; set; }
        public string FcNumber { get; set; }
        [JsonProperty("banners")]
        public IEnumerable<BannerItem> Banners { get; set; }
    }
}
