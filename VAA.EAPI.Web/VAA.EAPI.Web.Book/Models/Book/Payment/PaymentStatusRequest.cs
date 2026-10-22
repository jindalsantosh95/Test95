using MediatR;
using System.IO;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Models.Book.Payment
{
    public class PaymentStatusRequest : IRequest<PaymentStatusResponse>
    {
        public string BasketId { get; set; }
        public string PaymentReferenceId { get; set; }
        public string HppPaymentStatus { get; set; }
        public string AuthToken { get; set; }
        public PaymentStatusRequest()
        {
        }

        public PaymentStatusRequest(string basketId, string authToken, Stream body)
        {
            var fromJson = Json.Deserialize<PaymentStatusRequest>(new StreamReader(body).ReadToEndAsync().Result);

            BasketId = basketId;
            PaymentReferenceId = fromJson.PaymentReferenceId;
            HppPaymentStatus = fromJson.HppPaymentStatus;
            AuthToken = authToken;
        }
    }
}