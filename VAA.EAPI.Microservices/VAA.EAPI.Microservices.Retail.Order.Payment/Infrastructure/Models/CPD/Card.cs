using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Card
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "prefixes")]
        public Prefixes Prefixes { get; set; }

        [XmlAttribute(AttributeName = "type-id")]
        public string TypeId { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement(ElementName = "amount")]
        public Amount Amount { get; set; }

        [XmlElement(ElementName = "cvc")]
        public string Cvc { get; set; }

        [XmlElement(ElementName = "card-holder-name")]
        public string Cardholdername { get; set; }

        [XmlElement(ElementName = "card-number")]
        public string Cardnumber { get; set; }

        [XmlElement(ElementName = "expiry")]
        public string Expiry { get; set; }

        [XmlElement(ElementName = "pre_authenticated")]
        public bool PreAuthenticated { get; set; }

        [XmlElement(ElementName = "info-3d-secure")]
        public Info3DSecure Info3dsecure { get; set; }

        [XmlElement(ElementName = "address")]
        public Address Address { get; set; }
    }
}
