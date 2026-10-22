namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
{
    public class Journey
    {
        public string JourneyId { get; set; }
        public string Duration { get; set; }
        public IEnumerable<string> FlightSegmentRefIds { get; set; }
    }
}
