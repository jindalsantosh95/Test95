using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment.CoBrand;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Payment;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface IPaymentServiceProvider
    {
        Task<PaymentRefIdResponse> OrderPaymentReferenceId(OrderPaymentRequestMs request);
        Task<PaymentResponse> PaymentDetailsAsync(string paymentReferenceId, string paymentType);
        Task<PaymentResponse> PaymentCaptureAsync(PaymentCaputreRequest capturePaymentRequest);
        Task<PaymentResponse> PaymentVoidAsync(OrderPaymentVoidRequest voidRequest);
        Task<ApplicationStatusResponse> RetrieveApplicationStatusAsync(string token);
        Task<SynchronyTokenMSResponse> GetSynchronyDataShareTokenAsync(SynchronyTokenMSRequest tokenRequest);
        Task<EngagementScoreMSResponse> GetEngagementScoreAsync(EngagementScoreMSRequest request);
    }
}