using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Cryptogram
    {
        [XmlAttribute(AttributeName = "eci")]
        public string Eci { get; set; }

        [XmlAttribute(AttributeName = "xid")]
        public string Xid { get; set; }

        [XmlAttribute(AttributeName = "algorithm-id")]
        public string AlgorithmId { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
