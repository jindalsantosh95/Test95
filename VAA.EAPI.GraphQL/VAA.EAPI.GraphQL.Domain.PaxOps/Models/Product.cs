using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Product")]
    public class Product
    {
        public BookingType BookingType { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public TotalPrice TotalPrice { get; set; }
        public string Status { get; set; }
    }
}