using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Profile
    {
        [XmlElement(ElementName = "title")]
        public Title Title { get; set; }

        [XmlElement(ElementName = "first-name")]
        public FirstName FirstName { get; set; }

        [XmlElement(ElementName = "last-name")]
        public LastName LastName { get; set; }

        [XmlElement(ElementName = "type")]
        public Type Type { get; set; }

        [XmlElement(ElementName = "contact-info")]
        public ContactInfo ContactInfo { get; set; }

        [XmlElement(ElementName = "additional-data")]
        public AdditionalData AdditionalData { get; set; }
    }
}