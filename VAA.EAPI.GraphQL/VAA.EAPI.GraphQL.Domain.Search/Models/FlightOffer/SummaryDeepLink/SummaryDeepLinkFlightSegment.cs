using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink
{
    public class SummaryDeepLinkFlightSegment
    {
        public int? SliceNumber { get; set; }
        public string? MarketingCarrierCode { get; set; }
        public string? MarketingFlightNumber { get; set; }
        public string? OriginAirportCode { get; set; }
        public string? DestinationAirportCode { get; set; }
        public string? MarketingClassOfService { get; set; }
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public string? FareBasisCode { get; set; }
    }
}
