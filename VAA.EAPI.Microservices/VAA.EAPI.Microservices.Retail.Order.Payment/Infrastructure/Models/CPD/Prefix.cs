using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Prefix
    {
        [XmlElement(ElementName = "min")]
        public int Min { get; set; }

        [XmlElement(ElementName = "max")]
        public int Max { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }
    }
}
