using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("FlightDistanceTrip")]
    public class FlightDistance
    {
        public string Unit { get; set; }
        public int? Distance { get; set; }
    }
}
