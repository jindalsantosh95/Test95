using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo
{
    public sealed class SecureFlightInfoRequest
    {
        [GraphQLNonNullType]
        [GraphQLDescription("Booking PNR locator.")]
        public string PnrLocator { get; set; }

        [GraphQLNonNullType]
        [GraphQLDescription("Passenger given name(s) used to retrieve the booking.")]
        public string GivenNames { get; set; }

        [GraphQLNonNullType]
        [GraphQLDescription("Passenger surname used to retrieve the booking.")]
        public string Surname { get; set; }

        [GraphQLNonNullType]
        [GraphQLDescription("Passenger first name number from the Travel Requirements passenger context.")]
        public string PassengerFirstNin { get; set; }

        [GraphQLNonNullType]
        [GraphQLDescription("Passenger last name number from the Travel Requirements passenger context.")]
        public string PassengerLastNin { get; set; }
    }
}
