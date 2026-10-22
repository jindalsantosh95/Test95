using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Product
    {
        [XmlAttribute(AttributeName = "order-ref")]
        public string OrderRef { get; set; }

        [XmlAttribute(AttributeName = "sku")]
        public string Sku { get; set; }

        [XmlElement(ElementName = "type")]
        public Type Type { get; set; }

        [XmlElement(ElementName = "name")]
        public Name Name { get; set; }

        [XmlElement(ElementName= "airline-data")]
        public AirlineData AirlineData { get; set; }    
    }
}
