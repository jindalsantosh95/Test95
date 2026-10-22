using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "detailReasonTypeCode")]
    public class DetailReasonTypeCode
    {

        [XmlElement(ElementName = "FlightType")]
        public string FlightType { get; set; }
    }

}