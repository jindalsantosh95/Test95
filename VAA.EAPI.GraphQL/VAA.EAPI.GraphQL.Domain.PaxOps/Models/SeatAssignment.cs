using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:SeatAssignment")]
    public class SeatAssignment
    {
        public Seat Seat { get; set; }

        public static explicit operator SeatAssignment(Infrastructure.Models.CheckInMS.SeatAssignment source) => new()
        {
            Seat = source.Seat != null ? (Seat)source.Seat : null
        };
    }
}