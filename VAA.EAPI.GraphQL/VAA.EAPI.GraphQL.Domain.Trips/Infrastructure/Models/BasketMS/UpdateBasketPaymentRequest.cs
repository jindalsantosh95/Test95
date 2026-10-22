using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("tripsBasketPaymentRequest")]
    public class UpdateBasketPaymentRequest
    {
        public string BasketId { get; set; }
        public IEnumerable<PaymentSession> PaymentSessionsBasketMS { get; set; }

        public static explicit operator UpdateBasketPaymentRequest((PaymentStatusRequest request, string basketId, PaymentSession paymentSession) source)
        {
            var (request, basketId, paymentSession) = source;

            paymentSession.HppPaymentStatus = request?.PaymentStatus.ToString();

            return new()
            {
                BasketId = basketId,
                PaymentSessionsBasketMS = [paymentSession]
            };
        }
       
    }
}