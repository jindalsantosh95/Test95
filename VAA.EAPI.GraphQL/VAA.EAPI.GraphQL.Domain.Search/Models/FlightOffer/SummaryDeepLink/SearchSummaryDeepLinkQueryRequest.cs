namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink
{
    public class SearchSummaryDeepLinkQueryRequest
    {
        public string? MetaPartnerId { get; set; }
        public IEnumerable<SummaryDeepLinkFlightSegment>? FlightSegments { get; set; }
        public Amount? TotalOfferedAmount { get; set; }
        public PaxBreakdown? Customers { get; set; }
        public string? DominantSegmentBrandId { get; set; }
        public string? SummaryLink { get; set; }
    }
}
