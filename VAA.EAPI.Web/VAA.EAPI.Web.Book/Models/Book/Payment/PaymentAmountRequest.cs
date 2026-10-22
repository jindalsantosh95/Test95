using MediatR;
using System.IO;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Models.Book.Payment
{
    public class PaymentAmountRequest : IRequest<PaymentResponse>
    {
        public string BasketId { get; set; }
        public decimal AmountToPay { get; set; }
        public string Link { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public string AuthToken { get; set; }        

        public PaymentAmountRequest()
        {
        }

        public PaymentAmountRequest(string basketId, string authToken, string ipAddress, string browserInfo, Stream body)
        {
            var fromJson = Json.Deserialize<PaymentAmountRequest>(new StreamReader(body).ReadToEndAsync().Result);

            BasketId = basketId;
            IpAddress = ipAddress;
            BrowserInfo = browserInfo;
            AmountToPay = fromJson.AmountToPay;
            Link = fromJson.Link;
            AuthToken = authToken;
        }
    }
}