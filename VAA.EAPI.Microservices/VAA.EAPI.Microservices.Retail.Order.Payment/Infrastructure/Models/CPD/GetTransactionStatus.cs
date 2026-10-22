using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class GetTransactionStatus
    {
        [XmlElement(ElementName = "client-id")]
        public string ClientId { get; set; }

        [XmlElement(ElementName = "transactions")]
        public Transactions Transactions { get; set; }
    }
}
