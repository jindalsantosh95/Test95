using MediatR;
using VAA.EAPI.Web.Book.Models.Book.Payment;

namespace VAA.EAPI.Web.Book.Domain.Payment.Requests
{
    public class PaymentAmountRequestIdRequest : IRequest<PaymentResponse>
    {
        public string BasketId { get; set; }
        public decimal AmountToPay { get; set; }
        public string Link { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public string AuthToken { get; set; }
    }
}
