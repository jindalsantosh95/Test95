using NodaTime;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlightOffer
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
        public LocalDateTime? ScheduledDepartureLocalDateTime { get; set; }
        public string? FareBasisCode { get; set; }

        public static explicit operator DeepLinkSegment((SummaryDeepLinkFlightSegment flightSegment, int segmentId) source)
        {
            return new DeepLinkSegment
            {
                SliceNumber = source.flightSegment.SliceNumber,
                SegmentId = source.segmentId,
                MarketingCarrierCode = source.flightSegment.MarketingCarrierCode,
                MarketingFlightNumber = source.flightSegment.MarketingFlightNumber,
                OriginAirportCode = source.flightSegment.OriginAirportCode,
                DestinationAirportCode = source.flightSegment.DestinationAirportCode,
                MarketingClassOfService = source.flightSegment.MarketingClassOfService,
                ScheduledDepartureLocalDateTime = source.flightSegment.ScheduledDepartureLocalDateTime,
                FareBasisCode = source.flightSegment.FareBasisCode
            };
        }
    }
}
