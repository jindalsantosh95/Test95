using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record8
    {
        [JsonProperty("FORM_OF_PAYMENT_ONE")]
        public FormOfPayment FORM_OF_PAYMENT_ONE { get; set; }

        [JsonProperty("FORM_OF_PAYMENT_TWO")]
        public FormOfPayment FORM_OF_PAYMENT_TWO { get; set; }

    }
}
