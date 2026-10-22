using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class OriginDestinationAssociations
    {
        [JsonProperty("OdLabel")]
        public string OdLabel { get; set; }

        [JsonProperty("OriginDestination")]
        public OriginDestination OriginDestination { get; set; }
    }
}