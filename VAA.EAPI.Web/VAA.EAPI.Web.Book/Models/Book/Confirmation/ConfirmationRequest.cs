using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Web.Common.Entities;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class ConfirmationRequest : IRequest<Response<ConfirmationResponse>>
    {
        public string BasketId { get; set; }
        public string PaymentStatus { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public string AuthToken { get; set; }
        public bool FraudCheckConsentTaken { get; set; } = false;
        public Dictionary<string, string>? MetaPartner { get; set; }
        public ConfirmationRequest(string basketId, string authToken, string paymentStatus, string ipAddress, string browserInfo)
        {
            BasketId = basketId;
            PaymentStatus = paymentStatus;
            IpAddress = ipAddress;
            BrowserInfo = browserInfo;
            AuthToken = authToken;
        }

        public ConfirmationRequest(string basketId, string authToken, string paymentStatus, string ipAddress, string browserInfo, bool consentValue)
        {
            BasketId = basketId;
            PaymentStatus = paymentStatus;
            IpAddress = ipAddress;
            BrowserInfo = browserInfo;
            FraudCheckConsentTaken = consentValue;
            AuthToken = authToken;
        }

        public ConfirmationRequest(string basketId, string authToken, string paymentStatus, string ipAddress, string browserInfo, Dictionary<string, string> metapartner)
        {
            BasketId = basketId;
            PaymentStatus = paymentStatus;
            IpAddress = ipAddress;
            BrowserInfo = browserInfo;
            AuthToken = authToken;
            MetaPartner = metapartner;
        }

        public ConfirmationRequest(string basketId, string authToken, string paymentStatus, string ipAddress, string browserInfo, bool consentValue, Dictionary<string, string> metapartner)
        {
            BasketId = basketId;
            PaymentStatus = paymentStatus;
            IpAddress = ipAddress;
            BrowserInfo = browserInfo;
            FraudCheckConsentTaken = consentValue;
            AuthToken = authToken;
            MetaPartner = metapartner;
        }
    }
}
