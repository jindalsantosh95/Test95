using Newtonsoft.Json;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class FormOfPayment
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public string ExpiryDate { get; set; }
        public string AccountNumber { get; set; }
        public Payer Payer { get; set; }
        public string CardSecurityCode { get; set; }
        public Address Address { get; set; }
        public SecureCard3DAuthenticationInformationRecords SecureCard3DAuthenticationInformationRecords { get; set; }
        [JsonProperty("motoPayment")]
        public bool MotoPayment { get; set; }
        public IEnumerable<MerchantDefinedInformation> MerchantDefinedInformation {  get; set; }

    }
}
