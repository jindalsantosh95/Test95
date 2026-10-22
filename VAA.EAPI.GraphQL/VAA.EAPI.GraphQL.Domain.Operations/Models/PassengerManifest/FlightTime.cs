using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class FlightTime
    {
        public DateTimeOffset? Local { get; set; }
        [GraphQLName("UTC")]
        public DateTimeOffset? UTC { get; set; }
    }
}
