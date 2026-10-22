namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class DataList
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public IEnumerable<Penalty>? Penalty { get; set; }
        public IEnumerable<Journey> Journey { get; set; }
    }
}