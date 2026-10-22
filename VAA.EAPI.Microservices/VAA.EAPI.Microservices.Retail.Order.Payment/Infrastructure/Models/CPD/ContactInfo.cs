using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class ContactInfo
    {
        [XmlElement(ElementName ="email")]
        public Email Email { get; set; }
    }
}