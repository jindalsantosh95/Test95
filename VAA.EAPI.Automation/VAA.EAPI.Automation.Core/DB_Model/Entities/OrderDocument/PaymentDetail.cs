using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class PaymentDetail
    {
        [JsonProperty("FormOfPaymentId")]
        public string FormOfPaymentId { get; set; }

        [JsonProperty("FormOfPayments")]
        public string FormOfPayments { get; set; }

        [JsonProperty("Amount")]
        public Amount Amount { get; set; }

        [JsonProperty("CardMethod")]
        public CardMethod CardMethod { get; set; }

        [JsonProperty("Payer")]
        public Payer Payer { get; set; }

        [JsonProperty("PaymentReferenceId")]
        public string PaymentReferenceId { get; set; }

        [JsonProperty("MilesPoints")]
        public string MilesPoints { get; set; }

        [JsonProperty("MilesEquivalentPrice")]
        public MilesEquivalentPrice MilesEquivalentPrice { get; set; }
    }
}