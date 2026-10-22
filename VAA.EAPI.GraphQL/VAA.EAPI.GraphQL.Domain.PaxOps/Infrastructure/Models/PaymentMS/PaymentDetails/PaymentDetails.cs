using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.Customer;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails
{
    [GraphQLName("GraphqlOCIPaymentDetails")]

    public class PaymentDetails
    {
        public string FormOfPaymentId { get; set; }
        public PaymentType? FormOfPayments { get; set; }
        public Amount Amount { get; set; }
        public CardDetails CardMethod { get; set; }
        public PaymentPayer Payer { get; set; }
        public Address BillingAddress { get; set; }
        public string PaymentReferenceId { get; set; }
    }
}
