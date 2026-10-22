using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class InsurancePurchaseDetails
    {
        [JsonProperty("InsurancePurchaseStatus")]
        public InsurancePurchaseStatus InsurancePurchaseStatus { get; set; }

        [JsonProperty("InsurancePurchaseAmount")]
        public double InsurancePurchaseAmount { get; set; }

        [JsonProperty("TransactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("TimeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}