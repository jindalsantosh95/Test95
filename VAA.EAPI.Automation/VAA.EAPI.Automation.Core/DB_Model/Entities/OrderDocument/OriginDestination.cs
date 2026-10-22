using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class OriginDestination
    {
        [JsonProperty("OriginAirport")]
        public string OriginAirport { get; set; }

        [JsonProperty("DestinationAirport")]
        public string DestinationAirport { get; set; }
    }
}