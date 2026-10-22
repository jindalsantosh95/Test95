using System;
using System.Xml.Serialization;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop
{

    [XmlRoot(ElementName = "ENEStatus")]
    public class ENEStatus
    {

        [XmlElement(ElementName = "statusCode")]
        public string StatusCode { get; set; }
       
    }

}