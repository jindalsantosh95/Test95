using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity
{
    public class Request
    {
        [XmlAttribute(AttributeName = "postOrZipCode")]
        public string PostOrZipCode { get; set; }
        [XmlAttribute(AttributeName = "country")]
        public string Country { get; set; }
        [XmlAttribute(AttributeName = "streetDetails")]
        public string StreetDetails { get; set; }
    }

    [XmlRoot(ElementName = "Message", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
    public class Message
    {
        [XmlElement(ElementName = "Request", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
        public Request Request { get; set; }
        [XmlAttribute(AttributeName = "messageId")]
        public string MessageId { get; set; }
    }

    [XmlRoot(ElementName = "CompleteAddressLookup_RQ", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
    public class CompleteAddressLookup_RQ
    {
        [XmlElement(ElementName = "Message", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
        public Message Message { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}
