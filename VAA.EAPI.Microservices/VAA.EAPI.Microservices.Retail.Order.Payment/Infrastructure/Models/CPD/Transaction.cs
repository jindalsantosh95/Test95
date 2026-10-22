using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Transaction
    {        
        [XmlElement(ElementName = "orders")]
        public Orders Orders { get; set; }

        [XmlElement(ElementName = "amount")]
        public Amount Amount { get; set; }  

        [XmlElement(ElementName = "hmac")]
        public string Hmac { get; set; }

        [XmlElement(ElementName = "additional-data")]
        public AdditionalData AdditionalData { get; set; }

        [XmlAttribute(AttributeName = "card")]
        public string Card { get; set; }

        [XmlAttribute(AttributeName = "order-no")]
        public string OrderNo { get; set; }

        [XmlAttribute(AttributeName = "type-id")]
        public string TypeId { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }        

        [XmlAttribute(AttributeName = "store-card")]
        public string StoreCard { get; set; }

        [XmlAttribute(AttributeName = "booking-ref")]
        public string BookingRef { get; set; }
    }
}
