using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class ScheduledDepartureTime
    {
        [JsonProperty("LocalDateTime")]
        public DateTime LocalDateTime { get; set; }
    }
}