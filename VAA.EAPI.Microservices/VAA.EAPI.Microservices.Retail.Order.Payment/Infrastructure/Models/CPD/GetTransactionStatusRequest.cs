using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class GetTransactionStatusRequest
    {
        [XmlElement(ElementName = "get-transaction-status")]
        public GetTransactionStatus GetTransactionStatus { get; set; }
    }
}
