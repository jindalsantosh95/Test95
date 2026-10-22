using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Prefixes
    {
        [XmlElement(ElementName = "prefix")]
        public List<Prefix> Prefix { get; set; }
    }
}
