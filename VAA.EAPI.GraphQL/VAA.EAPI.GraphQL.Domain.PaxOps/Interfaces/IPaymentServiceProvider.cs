using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface IPaymentServiceProvider
    {
        Task<PaymentMsResponse> PaymentReferenceAsync(BaggagePaymentRequest request);
        Task<PaymentTransactionDetails> PaymentDetailsAsync(string paymentReferenceId, string paymentType);
    }
}
