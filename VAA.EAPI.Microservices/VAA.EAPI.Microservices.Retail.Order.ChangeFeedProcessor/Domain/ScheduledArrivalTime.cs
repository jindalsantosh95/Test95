using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class ScheduledArrivalTime
    {
        [JsonProperty("Local_Date_Time")]
        public string? Local_Date_Time { get; set; }

        [JsonProperty("UTC_Date_Time")]
        public string? UTC_Date_Time { get; set; }
    }
}
