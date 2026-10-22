using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class Securecard3dauthenticationinformationdetail
    {
        public string cavv { get; set; }
        public string version { get; set; }

        [JsonProperty("EciFlag")]
        public string EciFlag { get; set; }
        public string threeDSServerTransactionId { get; set; }
    }
}

