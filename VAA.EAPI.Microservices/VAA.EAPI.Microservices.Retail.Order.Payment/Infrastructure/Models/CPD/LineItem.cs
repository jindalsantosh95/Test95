using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class LineItem
    {
        [XmlElement(ElementName = "amount")]
        public Amount Amount { get; set; }

        [XmlElement(ElementName = "product")]
        public Product Product { get; set; }

    }
}
