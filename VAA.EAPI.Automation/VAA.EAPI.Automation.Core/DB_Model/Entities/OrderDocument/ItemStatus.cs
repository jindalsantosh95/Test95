using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class ItemStatus
    {
        [JsonProperty("TimeStamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }
    }
}