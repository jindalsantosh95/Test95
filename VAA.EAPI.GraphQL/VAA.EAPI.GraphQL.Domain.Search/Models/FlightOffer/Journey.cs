namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class Journey
    {
        public string JourneyId { get; set; }
        public string Duration { get; set; }
        public IEnumerable<string>? FlightSegmentRefIds { get; set; }
    }
}