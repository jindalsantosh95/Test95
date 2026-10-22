using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "statusCodes")]
    public class StatusCodes
    {

        [XmlElement(ElementName = "ENEStatus")]
        public ENEStatus ENEStatus { get; set; }
    }

}