using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Refund
    {
        [XmlElement(ElementName = "transaction")]
        public Transaction Transaction { get; set; }        

        [XmlAttribute(AttributeName = "client-id")]
        public string ClientId { get; set; }
    }
}
