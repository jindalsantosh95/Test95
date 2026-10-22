using System.Xml.Serialization;

namespace VAA.EAPI.NDC.Order.Domain
{
    public class EventTimeStamp
    {
        [XmlAttribute(AttributeName = "format")]
        public string Format { get; set; } = "yyyy-MM-dd hh:mm:ss.fffffffff";

        [XmlText]
        public string Value { get; set; }
    }
}
