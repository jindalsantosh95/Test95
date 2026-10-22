namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class OriginDestinationAssociation
    {
        public string OdLabel { get; set; }
        public OriginDestinationDetails OriginDestination { get; set; }
        public IEnumerable<string> JourneyRefIds { get; set; }
    }
}