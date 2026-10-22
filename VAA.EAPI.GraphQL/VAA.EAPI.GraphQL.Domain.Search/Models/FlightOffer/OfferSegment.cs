namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class OfferSegment
    {
        public string OfferSegmentId { get; set; }
        public string FlightSegmentRefId { get; set; }
        public string ClassOfService { get; set; }
        public CabinType CabinType { get; set; }
        public IEnumerable<string> ProductComponentRefIds { get; set; }
    }
}
