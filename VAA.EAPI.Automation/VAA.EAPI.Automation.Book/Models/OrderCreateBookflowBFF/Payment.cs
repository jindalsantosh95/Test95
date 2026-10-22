using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF
{
    public class Payment
    {
        public string ReturnUrl { get; set; }
        public string ExpiryUrl { get; set; }
        public string Link { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PayWithPointsValue { get; set; }
    }
}
