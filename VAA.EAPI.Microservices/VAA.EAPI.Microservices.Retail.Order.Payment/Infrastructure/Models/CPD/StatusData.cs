using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class StatusData
    {
        [XmlElement(ElementName = "status-message")]
        public List<StatusMessage> StatusMessage { get; set; }
    }
}
