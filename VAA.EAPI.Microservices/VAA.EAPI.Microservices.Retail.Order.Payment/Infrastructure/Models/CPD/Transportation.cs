using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Transportation
    {
        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }

        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }       

        [XmlElement(ElementName = "carriers")]
        public Carriers Carriers { get; set; }
    }
}