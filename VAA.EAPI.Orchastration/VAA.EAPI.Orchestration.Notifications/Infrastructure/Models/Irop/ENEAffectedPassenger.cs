using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ENEAffectedPassenger")]
    public class ENEAffectedPassenger
    {

        [XmlElement(ElementName = "passengerFirstName")]
        public string PassengerFirstName { get; set; }

        [XmlElement(ElementName = "passengerLastName")]
        public string PassengerLastName { get; set; }
    }

}