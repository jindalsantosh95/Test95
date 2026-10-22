using System.Xml.Serialization;
using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{
    [XmlRoot(ElementName = "contacts")]
    public class Contacts
    {

        [XmlElement(ElementName = "ENEContact")]
        public List<ENEContact> ENEContact { get; set; }
    }

}