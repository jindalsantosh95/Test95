using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class DataList
    {
        [JsonProperty("OriginDestinationAssociations")]
        public OriginDestinationAssociations[] OriginDestinationAssociations { get; set; }

        [JsonProperty("CustomerDetails")]
        public CustomerDetails[] CustomerDetails { get; set; }

        [JsonProperty("SegmentDetails")]
        public SegmentDetails[] SegmentDetails { get; set; }
    }
}