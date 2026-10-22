using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Profiles
    {
        [XmlElement(ElementName = "profile")]
        public List<Profile> Profile { get; set; }
    }
}