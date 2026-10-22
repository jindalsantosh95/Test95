using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:AirlineProduct")]
    public class AirlineProduct : Product
    {
        public IEnumerable<Slice> Slices { get; set; }
    }
}