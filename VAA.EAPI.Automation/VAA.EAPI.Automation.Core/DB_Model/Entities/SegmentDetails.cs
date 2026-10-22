using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class SegmentDetails
    {
        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("BookingClass")]
        public string BookingClass { get; set; }

        [JsonProperty("DepartureDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DepartureDate { get; set; }

        [JsonProperty("DestinationAirportcode")]
        public string DestinationAirportcode { get; set; }

        [JsonProperty("FareBasisCode")]
        public string FareBasisCode { get; set; }

        [JsonProperty("MarketingCarrierCode")]
        public string MarketingCarrierCode { get; set; }

        [JsonProperty("MarketingFlightNumber")]
        public string MarketingFlightNumber { get; set; }

        [JsonProperty("OperatingFlightNumber")]
        public string OperatingFlightNumber { get; set; }

        [JsonProperty("OriginAirportcode")]
        public string OriginAirportcode { get; set; }

        [JsonProperty("PartnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("SegmentMiles")]
        public string SegmentMiles { get; set; }

        [JsonProperty("StopOverCode")]
        public string StopOverCode { get; set; }

        [JsonProperty("TicketDesignator")]
        public string TicketDesignator { get; set; }

        [JsonProperty("CouponNumber")]
        public string CouponNumber { get; set; }

        [JsonProperty("FareBreakSequenceNumber")]
        public string FareBreakSequenceNumber { get; set; }

        [JsonProperty("SegmentId")]
        public string SegmentId { get; set; }

        [JsonProperty("FlightNumber")]
        public string FlightNumber { get; set; }
    }
}
