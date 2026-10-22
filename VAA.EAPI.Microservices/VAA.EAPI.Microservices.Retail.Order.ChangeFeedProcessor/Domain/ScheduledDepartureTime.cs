using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class ScheduledDepartureTime
    {
        [JsonProperty("DeparturePoint")]
        public string? Local_Date_Time { get; set; }

        [JsonProperty("DeparturePoint")]
        public string? UTC_Date_Time { get; set; }
    }
}
