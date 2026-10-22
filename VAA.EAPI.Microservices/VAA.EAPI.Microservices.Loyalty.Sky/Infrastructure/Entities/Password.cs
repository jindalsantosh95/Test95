using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Entities
{
    public class Password
    {
        [XmlAttribute] public string Type { get; set; }

        [XmlText] public string Value { get; set; }
    }
}
