using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity
{
    namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity
    {
        [XmlRoot(ElementName = "AddressDetails", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
        public class AddressDetails
        {
            [XmlAttribute(AttributeName = "companyName")]
            public string CompanyName { get; set; }
            [XmlAttribute(AttributeName = "building")]
            public string Building { get; set; }
            [XmlAttribute(AttributeName = "address1")]
            public string Address1 { get; set; }
            [XmlAttribute(AttributeName = "address2")]
            public string Address2 { get; set; }
            [XmlAttribute(AttributeName = "city")]
            public string City { get; set; }
            [XmlAttribute(AttributeName = "countyOrStateOrProvince")]
            public string CountyOrStateOrProvince { get; set; }
            [XmlAttribute(AttributeName = "postOrZipCode")]
            public string PostOrZipCode { get; set; }
            [XmlAttribute(AttributeName = "country")]
            public string Country { get; set; }
        }

        [XmlRoot(ElementName = "CompleteAddressDetails", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
        public class CompleteAddressDetails
        {
            [XmlElement(ElementName = "AddressDetails", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
            public AddressDetails AddressDetails { get; set; }
            [XmlAttribute(AttributeName = "pickLine")]
            public string PickLine { get; set; }
        }

        [XmlRoot(ElementName = "Message", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
        public class Message
        {
            [XmlElement(ElementName = "CompleteAddressDetails", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
            public List<CompleteAddressDetails> CompleteAddressDetails { get; set; }
            [XmlAttribute(AttributeName = "messageId")]
            public string MessageId { get; set; }
        }

        [XmlRoot(ElementName = "ResultType", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
        public class ResultType
        {
            [XmlAttribute(AttributeName = "messageId")]
            public string MessageId { get; set; }
            [XmlAttribute(AttributeName = "result")]
            public string Result { get; set; }
            [XmlAttribute(AttributeName = "description")]
            public string Description { get; set; }
            [XmlAttribute(AttributeName = "date")]
            public string Date { get; set; }
        }

        [XmlRoot(ElementName = "CompleteAddressLookup_RS", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
        public class CompleteAddressLookup_RS
        {
            [XmlElement(ElementName = "Message", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
            public Message Message { get; set; }
            [XmlElement(ElementName = "ResultType", Namespace = "http://schemas.virgin-atlantic.com/CustomerService/AddressLookup/Services/CompleteAddressLookup/2010/")]
            public ResultType ResultType { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }
    }
}
