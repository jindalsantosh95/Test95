using MediatR;
using System.IO;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Models.Book.Payment
{
    public class PaymentRefIdRequest : IRequest<PaymentResponse>
    {
        public string BasketId { get; set; }
        public string ReturnUrl { get; set; }
        public string ExpiryUrl { get; set; }
        public string Link { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public string AuthToken { get; set; }
        public string? PayWithPointsValue { get; set; }

        public PaymentRefIdRequest()
        {
        }

        public PaymentRefIdRequest(string basketId, string authToken, string ipAddress, string browserInfo, Stream body)
        {
            var fromJson = Json.Deserialize<PaymentRefIdRequest>(new StreamReader(body).ReadToEndAsync().Result);

            BasketId = basketId;
            IpAddress = ipAddress;
            BrowserInfo = browserInfo;
            ReturnUrl = fromJson.ReturnUrl;
            ExpiryUrl = fromJson.ExpiryUrl;
            Link = fromJson.Link;
            AuthToken = authToken;
            PayWithPointsValue = fromJson.PayWithPointsValue
                ;
        }
    }
}
