using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public class OrderDocument : BaseDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("OrderID")]
        public string? OrderId { get; set; }

        [JsonProperty("_ts")]
        public double TS { get; set; }

        [JsonProperty("CustomerDetails")]
        public IEnumerable<CustomerDetail>? CustomerDetails { get; set; }

        [JsonProperty("SegmentDetails")]
        public IEnumerable<SegmentDetail>? SegmentDetails { get; set; }
    }
}
