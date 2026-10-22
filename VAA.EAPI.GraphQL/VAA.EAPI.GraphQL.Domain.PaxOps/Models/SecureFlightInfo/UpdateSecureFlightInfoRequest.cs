using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo
{
    public sealed class UpdateSecureFlightInfoRequest
    {
        [GraphQLNonNullType]
        [GraphQLDescription("Secure flight reservation identifier returned by the secure flight information query.")]
        public string SecureFlightReservationId { get; set; }

        [GraphQLNonNullType]
        [GraphQLDescription("Passenger resource identifier returned by the secure flight information query.")]
        public string PassengerId { get; set; }

        [GraphQLDescription("Passenger first name number returned by the secure flight information query.")]
        public string PassengerFirstNin { get; set; }

        [GraphQLDescription("Passenger last name number returned by the secure flight information query.")]
        public string PassengerLastNin { get; set; }

        [GraphQLNonNullType]
        [GraphQLDescription("Secure flight details to add or update. Provide at least one detail. Canadian Travel Number and Redress Number cannot be provided together.")]
        public IEnumerable<SecureFlightDetailRequest> SecureFlightDetails { get; set; }

    }
}
