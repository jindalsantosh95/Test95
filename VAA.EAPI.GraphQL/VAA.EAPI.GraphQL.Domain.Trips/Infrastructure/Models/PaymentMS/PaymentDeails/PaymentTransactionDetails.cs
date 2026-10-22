using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails
{
    [GraphQLName("GraphqlTripsPaymentTransactionDetails")]
    public class PaymentTransactionDetails
    {
        public string PaymentReferenceId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public string ParentPaymentReferenceId { get; set; }
    }
}