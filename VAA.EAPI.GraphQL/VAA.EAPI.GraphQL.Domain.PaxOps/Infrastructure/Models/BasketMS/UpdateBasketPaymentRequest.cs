using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BasketMS
{
    [GraphQLName("ociBasketPaymentRequest")]
    public class UpdateBasketPaymentRequest
    {
        public string BasketId { get; set; }
        public IEnumerable<PaymentSession> PaymentSessionsBasketMS { get; set; }

        public static explicit operator UpdateBasketPaymentRequest((string basketId, string hppStatus) source)
        {
            return new()
            {
                BasketId = source.basketId,
                PaymentSessionsBasketMS = [new() { HppPaymentStatus = source.hppStatus }]
            };
        }
    }
}