using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class CreationDateTime
    {
        [JsonProperty("UtcDateTime")]
        public DateTime UtcDateTime { get; set; }
    }
}