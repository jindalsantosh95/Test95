using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class Contacts
    {
        [JsonProperty("Phone")]
        public string Phone { get; set; }

        [JsonProperty("PhoneAreaNumber")]
        public string PhoneAreaNumber { get; set; }
    }
}