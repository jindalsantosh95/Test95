using System.Xml.Serialization;

namespace VAA.EAPI.NDC.Order.Domain
{
    public class Indicator
    {
        [XmlElement(ElementName = "hasChanged")]
        public string HasChanged { get; set; } = "Y";

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}
