using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BasketMS
{
    [GraphQLName("ociBasketPaymentSession")]
    public class PaymentSession
    {
        public string HppPaymentStatus { get; set; }
    }
}