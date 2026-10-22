using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsBasketBooking")]
    public class Booking
    {
        public string? PnrLocator { get; set; }
        public Pos? Pos { get; set; }
        public IEnumerable<Person>? Passenger { get; set; }
        public IEnumerable<Slice>? Slices { get; set; }
        public IEnumerable<SeatAssignment> SeatDetails { get; set; }
        public string ExternalRloc { get; internal set; }
    }
}