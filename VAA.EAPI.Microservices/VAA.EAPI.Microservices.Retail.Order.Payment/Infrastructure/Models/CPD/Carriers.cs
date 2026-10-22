using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Carriers
    {
        [XmlElement(ElementName = "carrier")]
        public Carrier Carrier { get; set; }
    }
}