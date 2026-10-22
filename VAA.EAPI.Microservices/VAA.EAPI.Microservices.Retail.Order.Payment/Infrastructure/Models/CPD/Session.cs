using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Session
    {
        [XmlElement(ElementName = "amount")]
        public Amount Amount { get; set; }

        [XmlElement(ElementName = "status")]
        public string Status { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "total-amount")]
        public string TotalAmount { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }
}
