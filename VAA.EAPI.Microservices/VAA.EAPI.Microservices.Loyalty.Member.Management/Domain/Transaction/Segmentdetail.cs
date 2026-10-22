namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction
{
    public class Segmentdetail
    {
        public string SourceIdentifier { get; set; }
        public string SegmentId { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string BookingClass { get; set; }
        public string DepartureDate { get; set; }
        public string DestinationAirportcode { get; set; }
        public string FareBasisCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportcode { get; set; }
        public string PartnerCode { get; set; }
        public string SegmentMiles { get; set; }
        public string StopOverCode { get; set; }
        public string TicketDesignator { get; set; }
        public string CouponNumber { get; set; }
        public string FareBreakSequenceNumber { get; set; }
    }
}
