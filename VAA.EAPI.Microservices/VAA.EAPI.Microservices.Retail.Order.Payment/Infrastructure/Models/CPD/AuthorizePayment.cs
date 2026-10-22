using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Authorizepayment
    {
        [XmlElement(ElementName = "transaction")]
        public AuthorizeTransaction Transaction { get; set; }

        [XmlElement(ElementName = "client-info")]
        public Clientinfo Clientinfo { get; set; }

        [XmlAttribute(AttributeName = "account")]
        public string Account { get; set; }

        [XmlAttribute(AttributeName = "client-id")]
        public string ClientId { get; set; }
    }
}
