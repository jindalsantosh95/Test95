using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class InitializePaymentResponse
    {
        [XmlElement(ElementName = "transaction")]
        public Transaction Transaction { get; set; }

        [XmlElement(ElementName = "session")]
        public Session Session { get; set; }

        [XmlElement(ElementName = "cards")]
        public Cards Cards { get; set; }

        [XmlElement(ElementName = "status")]
        public List<Status> Status { get; set; }
    }
}
