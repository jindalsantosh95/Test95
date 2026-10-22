using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class InsurancePurchaseStatus
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}