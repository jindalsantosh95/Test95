using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class CaptureTransaction
    {
        [XmlElement(ElementName = "orders")]
        public Orders Orders { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "order-no")]
        public string OrderNo { get; set; }
    }
}
