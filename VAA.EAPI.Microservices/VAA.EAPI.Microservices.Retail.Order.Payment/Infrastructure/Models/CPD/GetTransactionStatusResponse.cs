using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class GetTransactionStatusResponse
    {
        [XmlElement(ElementName = "transaction")]
        public GetStatusTransaction Transaction { get; set; }

        [XmlElement(ElementName = "payment_status")]
        public string PaymentStatus { get; set; }
    }
}
