using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Carrier
    {
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }

        [XmlElement(ElementName = "number")]
        public Number Number { get; set; }
    }
}