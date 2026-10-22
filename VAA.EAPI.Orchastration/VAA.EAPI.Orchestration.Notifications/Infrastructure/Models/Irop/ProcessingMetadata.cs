using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ProcessingMetadata")]
    public class ProcessingMetadata
    {

        [XmlElement(ElementName = "EmailAddress")]
        public string EmailAddress { get; set; }

        [XmlElement(ElementName = "FromAddress")]
        public string FromAddress { get; set; }

        [XmlElement(ElementName = "Template")]
        public string Template { get; set; }
    }

}