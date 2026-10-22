using System.Xml.Serialization;
using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{
    [XmlRoot(ElementName = "affectedPassenger")]
    public class AffectedPassenger
    {

        [XmlElement(ElementName = "ENEAffectedPassenger")]
        public List<ENEAffectedPassenger> ENEAffectedPassenger { get; set; }
    }

}