using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class AirlineData
    {
        [XmlElement(ElementName = "trips")]
        public Trips Trips { get; set; }

        [XmlElement(ElementName = "profiles")]
        public Profiles Profiles { get; set; }
    }
}