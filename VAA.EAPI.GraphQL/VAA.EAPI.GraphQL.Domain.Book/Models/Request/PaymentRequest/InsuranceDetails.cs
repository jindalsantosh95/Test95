using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest
{
    [GraphQLName("insurancePaymentDetails")]
    public class InsuranceDetails
    {
        public Amount Amount { get; set; }
    }
}