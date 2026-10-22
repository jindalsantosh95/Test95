using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class GetStatusTransaction
    {
        [XmlAttribute(AttributeName = "accoutid")]
        public string AccountId { get; set; }

        [XmlAttribute(AttributeName = "approval-code")]
        public string ApprovalCode { get; set; }

        [XmlAttribute(AttributeName = "card-id")]
        public string CardId { get; set; }

        [XmlAttribute(AttributeName = "clientid")]
        public string ClientId { get; set; }

        [XmlAttribute(AttributeName = "extid")]
        public string ExtId { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string TransactionId { get; set; }

        [XmlAttribute(AttributeName = "language")]
        public string Language { get; set; }

        [XmlAttribute(AttributeName = "mpoint-id")]
        public string MPointId { get; set; }

        [XmlAttribute(AttributeName = "order-no")]
        public string OrderId { get; set; }

        [XmlAttribute(AttributeName = "payment-method-id")]
        public string PaymentMethodId { get; set; }

        [XmlAttribute(AttributeName = "psp-id")]
        public string PspId { get; set; }

        [XmlAttribute(AttributeName = "session-id")]
        public string SessionId { get; set; }

        [XmlAttribute(AttributeName = "session-type")]
        public string SessionType { get; set; }

        [XmlAttribute(AttributeName = "txn-type-id")]
        public string TransactionTypeId { get; set; }

        [XmlAttribute(AttributeName = "walletid")]
        public string WalletId { get; set; }

        [XmlElement(ElementName = "amount")]
        public Amount Amount { get; set; }

        [XmlElement(ElementName = "card-mask")]
        public string MaskedCardNumber { get; set; }

        [XmlElement(ElementName = "card-expiry")]
        public string CardExpiry { get; set; }

        [XmlElement(ElementName = "card-name")]
        public string CardName { get; set; }

        [XmlElement(ElementName = "psp-name")]
        public string PspName { get; set; }

        [XmlElement(ElementName = "auto-redirect")]
        public string AutoRedirect { get; set; }

        [XmlElement(ElementName = "createdDate")]
        public string CreatedDate { get; set; }

        [XmlElement(ElementName = "createdTime")]
        public string CreatedTime { get; set; }

        [XmlElement(ElementName = "status")]
        public StatusData Status { get; set; }

        [XmlElement(ElementName = "sign")]
        public string Sign { get; set; }

        [XmlElement(ElementName = "client-info")]
        public Clientinfo Clientinfo { get; set; }

        [XmlElement(ElementName = "additional-data")]
        public AdditionalData AdditionalData { get; set; }

        [XmlElement(ElementName = "routeconfig-id")]
        public string RouteConfigId { get; set; }

    }
}
