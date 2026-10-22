using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ENESeat")]
    public class ENESeat
    {

        [XmlElement(ElementName = "passengerSeatNumber")]
        public string PassengerSeatNumber { get; set; }
    }

}