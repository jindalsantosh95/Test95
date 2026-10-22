using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class OrderDetails
    {
        [JsonProperty("OrderDetails")]
        public OrderDocuments OrderDocuments { get; set; }

        [JsonProperty("SegmentDetails")]
        public SegmentDetails[] SegmentDetails { get; set; }

    }
}
