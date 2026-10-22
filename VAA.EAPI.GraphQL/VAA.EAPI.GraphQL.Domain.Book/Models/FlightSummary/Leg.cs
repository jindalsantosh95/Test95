using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryLeg")]
    public class Leg
    {
        public Origin? Origin { get; set; }
        public Destination? Destination { get; set; }
        public string? Duration { get; set; }
        public LocalDateTime? Departure { get; set; }
        public LocalDateTime? Arrival { get; set; }
        public string? StopOver { get; set; }
        public bool IsDominantLeg { get; set; }
        public Airline? OperatingAirline { get; set; }
        public Airline? MarketingAirline { get; set; }



    }
}