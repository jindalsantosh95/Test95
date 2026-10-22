using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    [GraphQLName("TripsSeatBookingReference")]
    public class BookingReference
    {
        public string Rloc { get; set; }
    }
}