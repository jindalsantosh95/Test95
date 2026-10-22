namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class DisruptionSegment
    {
        public string SegmentId { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string ScheduledDepartureDateTime { get; set; }
        public string ScheduledArrivalDateTime { get; set; }
        public string SegmentStatus { get; set; }
        public string CouponStatus { get; set; }
        public bool ScheduleChangeIndicator { get; set; }
    }
}
