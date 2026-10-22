using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class UpdateOrderData
    {
        [XmlElement(ElementName = "transaction")]
        public CaptureTransaction Transaction { get; set; }

    }
}
