using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.FareInfo
{
    public class FareRuleSegment
    {
        public SegmentOrigin Origin { get; set; }
        public SegmentDestination Destination { get; set; }

        public FareRuleSegment(FlightOffer.Segment segment, IEnumerable<AirportInfo> airportDetails)
        {
            var origin = airportDetails.First(x => x.AirportCode == segment.OriginAirportCode);
            var destination = airportDetails.First(x => x.AirportCode == segment.DestinationAirportCode);

            Origin = (SegmentOrigin)origin;
            Destination = (SegmentDestination)destination;
        }
    }
}
