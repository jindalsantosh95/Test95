namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class DeepLinkSegment
    {
        public int? SliceNumber { get; set; }
        public int? SegmentId { get; set; }
        public string? MarketingCarrierCode { get; set; }
        public string? MarketingFlightNumber { get; set; }
        public string? OriginAirportCode { get; set; }
        public string? DestinationAirportCode { get; set; }
        public string? MarketingClassOfService { get; set; }
        public string? ScheduledDepartureLocalDateTime { get; set; }
        public string? FareBasisCode { get; set; }
    }
}
