using MediatR;
using VAA.EAPI.Web.Book.Models.Book.Payment;

namespace VAA.EAPI.Web.Book.Domain.Payment.Requests
{
    public class PaymentStatusGraphQLRequest : IRequest<PaymentStatusResponse>
    {
        public string BasketId { get; set; }
        public string PaymentReferenceId { get; set; }
        public string HppPaymentStatus { get; set; }
        public string AuthToken { get; set; }
    }
}
