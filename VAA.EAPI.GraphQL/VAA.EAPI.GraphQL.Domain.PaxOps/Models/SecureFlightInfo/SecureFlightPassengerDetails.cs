using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo
{
    [GraphQLName("SecureFlightPassengerDetails")]
    public sealed class SecureFlightPassengerDetails
    {
        public string PassengerId { get; set; }
        public string PassengerFirstNin { get; set; }
        public string PassengerLastNin { get; set; }
        public string? KnownTravellerNumber { get; set; }
        public string? CanadianTravelNumber { get; set; }
        public string? RedressNumber { get; set; }
    }
}
