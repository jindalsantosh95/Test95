using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class HealthPassport
    {
        [JsonProperty("PassengerNameNumber")]
        public string? PassengerNameNumber { get; set; }

        [JsonProperty("PassengerFirstName")]
        public string? PassengerFirstName { get; set; }

        [JsonProperty("PassengerLastName")]
        public string? PassengerLastName { get; set; }

        [JsonProperty("HealthPassportSource")]
        public string? HealthPassportSource { get; set; }

        [JsonProperty("HealthPassportStatus")]
        public string? HealthPassportStatus { get; set; }
    }
}
