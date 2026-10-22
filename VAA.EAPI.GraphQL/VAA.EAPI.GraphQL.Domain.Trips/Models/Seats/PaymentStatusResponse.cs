using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("tripsPaymentStatusResponse")]
    public class PaymentStatusResponse
    {
        public PaymentStatus PaymentStatus { get; set; }
    }
}