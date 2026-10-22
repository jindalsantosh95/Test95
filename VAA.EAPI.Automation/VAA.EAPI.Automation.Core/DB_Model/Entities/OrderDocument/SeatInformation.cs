using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class SeatInformation
    {
        [JsonProperty("FlightLegId")]
        public string FlightLegId { get; set; }

        [JsonProperty("SeatColumnName")]
        public string SeatColumnName { get; set; }

        [JsonProperty("SeatRowNum")]
        public string SeatRowNum { get; set; }

        [JsonProperty("EconomyComfort")]
        public bool EconomyComfort { get; set; }

        [JsonProperty("ExitRow")]
        public bool ExitRow { get; set; }

        [JsonProperty("ExitOverride")]
        public bool ExitOverride { get; set; }

        [JsonProperty("SeatStatus")]
        public string SeatStatus { get; set; }
    }
}