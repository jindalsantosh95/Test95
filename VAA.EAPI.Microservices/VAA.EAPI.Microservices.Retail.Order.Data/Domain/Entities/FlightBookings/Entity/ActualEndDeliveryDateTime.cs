

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class ActualEndDeliveryDateTime
    {
        public string LocalDateTime { get; set; }
        public string UTCDateTime { get; set; }

        // Input-only SCV aliases preserve the existing API serialization names.
        [Newtonsoft.Json.JsonProperty("UTC_Date_Time")]
        private string ScvUtcDateTime { set => UTCDateTime = value; }

        [Newtonsoft.Json.JsonProperty("Local_Date_Time")]
        private string ScvLocalDateTime { set => LocalDateTime = value; }
    }
}
