using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "BookingClass")]
    public class BookingClass
    {

        [XmlElement(ElementName = "upper")]
        public string Upper { get; set; }

        [XmlElement(ElementName = "directbooking")]
        public string Directbooking { get; set; }
    }

}