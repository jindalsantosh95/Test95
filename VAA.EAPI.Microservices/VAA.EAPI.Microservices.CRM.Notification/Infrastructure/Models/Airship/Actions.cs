using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Models.Airship
{
    public class Actions
    {
        public Open Open { get; set; }

        [JsonProperty("add_custom_event")]
        public CustomEvent AddCustomEvent { get; set; }
    }
}
