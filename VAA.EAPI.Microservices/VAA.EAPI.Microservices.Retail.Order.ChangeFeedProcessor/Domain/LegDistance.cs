using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class LegDistance
    {
        [JsonProperty("Distance")]
        public string? Distance { get; set; }

        [JsonProperty("Unit")]
        public string? Unit { get; set; }
    }
}
