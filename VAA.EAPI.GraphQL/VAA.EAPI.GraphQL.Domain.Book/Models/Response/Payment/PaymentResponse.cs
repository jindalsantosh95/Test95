using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment
{
    public class PaymentResponse
    {
        public string PaymentReferenceId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public string ParentPaymentReferenceId { get; set; }
    }
}
