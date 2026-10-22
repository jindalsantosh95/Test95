using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class Services
    {
        [JsonProperty("CustomerRefIds")]
        public IEnumerable<String> CustomerRefIds { get; set; }

        [JsonProperty("OriginDestinationRefIds")]
        public IEnumerable<String> OriginDestinationRefIds { get; set; }

        [JsonProperty("SegmentRefIds")]
        public IEnumerable<String> SegmentRefIds { get; set; }

        [JsonProperty("SeatInformation")]
        public SeatInformation SeatInformation { get; set; }
    }
}