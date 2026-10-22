using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Address
    {
        [XmlAttribute(AttributeName = "country-id")]
        public string CountryId { get; set; }

        [XmlElement(ElementName = "first-name")]
        public string FirstName { get; set; }

        [XmlElement(ElementName = "last-name")]
        public string LastName { get; set; }

        [XmlElement(ElementName = "street")]
        public string Street { get; set; }

        [XmlElement(ElementName = "postal-code")]
        public string PostalCode { get; set; }

        [XmlElement(ElementName = "city")]
        public string City { get; set; }

        [XmlElement(ElementName = "contact-details")]
        public Clientinfo ContactDetails { get; set; }

    }
}