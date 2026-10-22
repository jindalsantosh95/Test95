using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy
{

    //Sample
    //<?xml version="1.0" encoding="UTF-8"?>
    //<root>
    //<status code = "9400" >
    // < !--For Exceptions-->
    // <exception type = "http" >
    // < code > 400 </ code >
    // </ exception >
    // </ status >
    //</ root >


    [XmlRoot(ElementName = "voidTransactionErrorResponse")]
    public class VoidTransactionErrorResponse
    {

        [XmlElement(ElementName = "status")]
        public Status Status { get; set; }
    }

    [XmlRoot(ElementName = "exception")]
    public class Exception
    {

        [XmlElement(ElementName = "code")]
        public int Code { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlText]
        public int Text { get; set; }
    }

    [XmlRoot(ElementName = "status")]
    public class Status
    {

        [XmlElement(ElementName = "exception")]
        public Exception Exception { get; set; }

        [XmlAttribute(AttributeName = "code")]
        public int Code { get; set; }

        [XmlText]
        public int Text { get; set; }
    }
}
