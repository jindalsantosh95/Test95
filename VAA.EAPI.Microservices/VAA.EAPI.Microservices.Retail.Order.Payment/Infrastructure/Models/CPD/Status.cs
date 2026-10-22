using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Status
    {
        [XmlElement(ElementName = "exception")]
        public Exception Exception { get; set; }

        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }

        [XmlAttribute(AttributeName = "sub-code")]
        public string SubCode { get; set; }
      
        [XmlText]
        public string Text { get; set; }
    }
}
