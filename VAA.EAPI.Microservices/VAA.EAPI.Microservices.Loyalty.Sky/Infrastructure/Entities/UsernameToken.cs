using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Entities
{
    [XmlRoot(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]

    public class UsernameToken
    {
        [XmlElement] public string Username { get; set; }

        [XmlElement] public Password Password { get; set; }
    }
}
