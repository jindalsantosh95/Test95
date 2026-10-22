using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails
{
    [GraphQLName("GraphqlOCIPaymentTransactionDetails")]
    public class PaymentTransactionDetails
    {
        public string PaymentReferenceId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public string ParentPaymentReferenceId { get; set; }
    }
}
