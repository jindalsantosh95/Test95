using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "MessageData")]
    public class MessageData
    {

        [XmlElement(ElementName = "ENENotification")]
        public ENENotification ENENotification { get; set; }

        [XmlElement(ElementName = "BookingClass")]
        public BookingClass BookingClass { get; set; }
    }

}