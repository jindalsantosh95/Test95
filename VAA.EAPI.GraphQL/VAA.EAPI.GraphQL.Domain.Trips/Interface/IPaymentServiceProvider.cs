using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IPaymentServiceProvider
    {
        Task<PaymentRefIdResponseMs> PaymentReferenceId(PaymentRequestMs request);
        Task<PaymentTransactionDetails> PaymentDetailsAsync(string paymentReferenceId, string paymentType);
        Task<HolidayPaymentResponseMs> HolidayPaymentAsync(HolidayPaymentRequestMs request);
    }
}
