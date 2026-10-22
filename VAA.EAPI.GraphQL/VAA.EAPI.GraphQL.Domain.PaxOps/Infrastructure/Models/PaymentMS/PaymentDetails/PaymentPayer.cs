using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails
{
    [GraphQLName("GraphqlOCIPaymentPayer")]
    public class PaymentPayer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
