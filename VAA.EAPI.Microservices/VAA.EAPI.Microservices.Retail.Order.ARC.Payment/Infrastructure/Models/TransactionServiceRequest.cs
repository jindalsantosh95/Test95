using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class TransactionServiceRequest
    {
        [JsonProperty("TSCD")]
        public string TSCD { get; set; }

        [JsonProperty("TPST")]
        public string TPST { get; set; }

        [JsonProperty("transactions")]
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}
