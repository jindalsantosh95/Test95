using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class BookingReference
    {
        [JsonProperty("AirlineDesignator")]
        public string AirlineDesignator { get; set; }

        [JsonProperty("RLOC")]
        public string RLOC { get; set; }

        [JsonProperty("RLOCCreateDate")]
        public DateTime RLOCCreateDate { get; set; }
    }
}