using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "flights")]
    public class Flights
    {

        [XmlElement(ElementName = "ENEFlight")]
        public ENEFlight ENEFlight { get; set; }
    }

}