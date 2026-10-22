using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class ReportedFormOfPayment
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]

        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public Payer Payer { get; set; }
        public string ExpiryDate { get; set; }
        public string AccountNumber { get; set; }
        public string CardSecurityCode { get; set; }
    }
}
