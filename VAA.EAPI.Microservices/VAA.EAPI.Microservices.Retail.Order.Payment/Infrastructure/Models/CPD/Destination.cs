using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Destination
    {
        [XmlAttribute(AttributeName = "time-zone")]
        public string TimeZone { get; set; }

        [XmlAttribute(AttributeName = "country-id")]
        public string CountryId { get; set; }

        [XmlAttribute(AttributeName = "external-id")]
        public string ExternalId { get; set; }
    }
}