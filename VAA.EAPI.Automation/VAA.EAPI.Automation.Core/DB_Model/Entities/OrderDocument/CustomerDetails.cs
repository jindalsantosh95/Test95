using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class CustomerDetails
    {
        [JsonProperty("CustId")]
        public string CustId { get; set; }

        [JsonProperty("Ptc")]
        public string Ptc { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("Contacts")]
        public Contacts Contacts { get; set; }
    }
}