using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo
{
    public sealed class SecureFlightDetailRequest
    {
        [GraphQLNonNullType]
        public SecureFlightDetailType Type { get; set; }

        [GraphQLNonNullType]
        public string Number { get; set; }
    }
}
