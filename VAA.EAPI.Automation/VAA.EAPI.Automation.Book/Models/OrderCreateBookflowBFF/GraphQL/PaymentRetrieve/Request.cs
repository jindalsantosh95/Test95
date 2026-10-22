using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.PaymentRetrieve
{
    public class Request
    {
        public string basketId { get; set; }
        public double amountToPay { get; set; }
        public string deviceInfo { get; set; }
        public string ipAddress { get; set; }
        public string browserInfo { get; set; }
        public string paymentSuccessLink { get; set; }
        public string paymentFailureLink { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string payWithPointsValue { get; set; }
    }
}
