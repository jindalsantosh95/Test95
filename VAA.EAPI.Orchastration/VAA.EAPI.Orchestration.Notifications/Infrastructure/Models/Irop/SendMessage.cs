using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "SendMessage")]
    public class SendMessage
    {

        [XmlElement(ElementName = "ProcessingMetadata")]
        public ProcessingMetadata ProcessingMetadata { get; set; }

        [XmlElement(ElementName = "MessageData")]
        public MessageData MessageData { get; set; }
    }

}