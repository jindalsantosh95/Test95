using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class AuthorizePaymentRequest
    {
        [XmlElement(ElementName = "authorize-payment")]
        public Authorizepayment Authorizepayment { get; set; }
    }
}
