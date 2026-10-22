using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsBasketMSPassengerSeatMap")]
    public class FlightPassengerSeatMap
    {
        public string SectorId { get; set; }
        public string SeatOfferId { get; set; }
        public string SelectedSeatOfferId { get; set; }
        public string Pos { get; set; }
        public string PassengerId { get; set; }
        public string SeatNumber { get; set; }
        public SeatPrice Price { get; set; }
        public string LinkedInfantId { get; set; }

        public static explicit operator FlightPassengerSeatMap((Segment segment, Passenger passenger) source)
        {
            var (segment, passenger) = source;

            return new()
            {
                SectorId = segment.Id,
                PassengerId = passenger.PassengerId,
                SeatNumber = string.Empty,
                SeatOfferId = string.Empty,
                SelectedSeatOfferId = string.Empty,
                Pos = string.Empty,
                Price = new(),
                LinkedInfantId = passenger.AssociatedInfantId
            };
        }
    }
}