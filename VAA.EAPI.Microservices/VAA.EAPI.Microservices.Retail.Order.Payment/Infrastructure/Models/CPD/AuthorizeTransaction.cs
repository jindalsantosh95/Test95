using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class AuthorizeTransaction
    {
        [XmlElement(ElementName = "card")]
        public Card Card { get; set; }

        [XmlAttribute(AttributeName = "type-id")]
        public string TypeId { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }
}
