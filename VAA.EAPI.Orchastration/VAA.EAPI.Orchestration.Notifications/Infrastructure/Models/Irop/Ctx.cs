using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ctx")]
    public class Ctx
    {

        [XmlElement(ElementName = "SendMessage")]
        public SendMessage SendMessage { get; set; }
    }

}