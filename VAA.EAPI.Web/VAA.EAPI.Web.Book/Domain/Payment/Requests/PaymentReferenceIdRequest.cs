using MediatR;
using VAA.EAPI.Web.Book.Models.Book.Payment;

namespace VAA.EAPI.Web.Book.Domain.Payment.Requests
{
    public class PaymentReferenceIdRequest : IRequest<PaymentResponse>
    {
        public string BasketId { get; set; }
        public string ReturnUrl { get; set; }
        public string ExpiryUrl { get; set; }
        public string Link { get; set; }
        public decimal AmountToPay { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public string AuthToken { get; set; }
        public string? PayWithPointsValue { get; set; }
    }
}