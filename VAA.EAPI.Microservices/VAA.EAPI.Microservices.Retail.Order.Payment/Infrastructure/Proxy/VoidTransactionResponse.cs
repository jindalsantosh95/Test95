using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy
{
    //   <? xml version="1.0" encoding="UTF-8"?>
    //<root>
    //<status code = "1000" />
    //</ root >
    [XmlRoot(ElementName = "voidTransactionResponse")]
    public class VoidTransactionResponse
    {

        [XmlElement(ElementName = "status")]
        public Status Status { get; set; }
    }
    //[XmlRoot(ElementName = "status")]
    //public class Status
    //{

    //    [XmlAttribute(AttributeName = "code")]
    //    public int Code { get; set; }
    //}
}
