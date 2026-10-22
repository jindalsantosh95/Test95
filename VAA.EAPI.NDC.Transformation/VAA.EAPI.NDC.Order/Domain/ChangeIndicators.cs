using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.NDC.Order.Domain
{
    public class ChangeIndicators
    {
        [XmlElement(ElementName = "Indicator")]
        public List<Indicator> Indicator { get; set; }
    }
}
