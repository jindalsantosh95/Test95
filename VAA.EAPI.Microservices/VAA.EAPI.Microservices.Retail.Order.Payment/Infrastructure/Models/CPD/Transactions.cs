using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Transactions
    {
        [XmlElement(ElementName = "transaction-id")]
        public string transactionId { get; set; }
    }
}
