using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Booking")]
    public class Booking
    {
        public string Status { get; set; }
        public string BookingId { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Product> Product { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}