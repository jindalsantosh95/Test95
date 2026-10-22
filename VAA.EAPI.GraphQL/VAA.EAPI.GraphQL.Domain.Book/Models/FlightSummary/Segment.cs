using HotChocolate;
using NodaTime;


namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummarySegment")]
    public class Segment
    {
        public IEnumerable<Metal>? Metal { get; set; }
        public Airline? Airline { get; set; }
        public string? FlightNumber { get; set; }
        public string? OperatingFlightNumber { get; set; }
        public Airline? OperatingAirline { get; set; }
        public Origin? Origin { get; set; }
        public Destination? Destination { get; set; }
        public string? Duration { get; set; }
        public LocalDateTime? Departure { get; set; }
        public LocalDateTime? Arrival { get; set; }
        public int StopCount { get; set; }
        public string? Connection { get; set; }
        public IEnumerable<Leg>? Legs { get; set; }
        public string? BookingClass { get; set; }
        public string? FareBasisCode { get; set; }
        public string? DominantFareProduct { get; set; }

    }
}