using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Initializepayment
    {
        [XmlElement(ElementName = "transaction")]
        public Transaction Transaction { get; set; }

        [XmlElement(ElementName = "auth-token")]
        public string Authtoken { get; set; }

        [XmlElement(ElementName = "client-info")]
        public Clientinfo Clientinfo { get; set; }

        [XmlAttribute(AttributeName = "account")]
        public string Account { get; set; }

        [XmlAttribute(AttributeName = "client-id")]
        public string ClientId { get; set; }
    }
}
