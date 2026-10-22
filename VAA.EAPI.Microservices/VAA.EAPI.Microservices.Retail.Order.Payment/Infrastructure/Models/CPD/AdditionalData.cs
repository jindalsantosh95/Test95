using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class AdditionalData
    {
        [XmlElement(ElementName = "param")]
        public List<Param> Param { get; set; }
    }
}
