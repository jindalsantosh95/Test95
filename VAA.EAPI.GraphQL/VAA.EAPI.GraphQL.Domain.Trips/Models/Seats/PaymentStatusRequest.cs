using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("tripsPaymentStatusRequest")]
    public class PaymentStatusRequest
    {
        public string TripReservationId { get; set; }
        public string PaymentReferenceId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}