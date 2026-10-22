using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryFlight")]
    public class Flight
    {
        public string? FlightId { get; set; }
        public IEnumerable<Segment>? Segments { get; set; }
        public string? Duration { get; set; }
        public string? FareFamilyType { get; set; }
        public FareFamilyFees? FareFamilyFees { get; set; }
        public string? CabinName { get; set; }
        public Origin? Origin { get; set; }
        public LocalDateTime? Arrival { get; set; }
        public Destination? Destination { get; set; }
        public LocalDateTime? Departure { get; set; }
        public string? Id { get; set; }
        public string? FareId { get; set; }
    }
}