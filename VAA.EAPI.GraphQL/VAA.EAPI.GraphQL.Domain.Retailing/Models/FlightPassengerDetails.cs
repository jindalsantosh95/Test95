using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    [GraphQLName("Retailing.FlightPaxDetails")]
    public class FlightPassengerDetails
    {
        public string RecordLocator { get; set; }
        public DateTime DepartureDate { get; set; }
        public FlightNumber OperatingFlight { get; set; }
        public FlightNumber MarketingFlight { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
    }
}
