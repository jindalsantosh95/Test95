namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
{
    public class OriginDestinationAssociation
    {
        public string OdLabel { get; set; }
        public OriginDestination OriginDestination { get; set; }
        public IEnumerable<string> JourneyRefIds { get; set; }
    }
}
