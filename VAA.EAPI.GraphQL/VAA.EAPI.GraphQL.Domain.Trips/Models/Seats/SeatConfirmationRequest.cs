using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("tripSeatConfirmationRequest")]
    public class SeatConfirmationRequest
    {
        public string TripReservationId { get; set; }
        public string? ChildPaymentReferenceId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }
}