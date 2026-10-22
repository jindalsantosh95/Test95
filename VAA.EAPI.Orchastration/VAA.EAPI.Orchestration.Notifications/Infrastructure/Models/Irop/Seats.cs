using System.Xml.Serialization;
using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "seats")]
    public class Seats
    {

        [XmlElement(ElementName = "ENESeat")]
        public List<ENESeat> ENESeat { get; set; }
    }

}