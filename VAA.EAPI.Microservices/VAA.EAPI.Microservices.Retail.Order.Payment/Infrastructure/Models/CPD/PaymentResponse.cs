using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class PaymentResponse
    {
        [XmlElement(ElementName = "status")]
        public List<Status> Status { get; set; }
    }
}
