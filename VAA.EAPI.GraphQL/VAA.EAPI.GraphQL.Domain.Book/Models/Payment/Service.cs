namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class Service
    {
        public IEnumerable<string> JourneyRefIds { get; set; }
        public IEnumerable<string> FlightSegmentsRefIds { get; set; }
        public FareDetail FareDetails { get; set; }
    }
}
