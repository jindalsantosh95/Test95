using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlexiDateSearch
{
    [GraphQLName("OriginDestinationFlexiSearch")]
    public class OriginDestination
    {
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public string DepartureDate { get; set; }
    }
}
