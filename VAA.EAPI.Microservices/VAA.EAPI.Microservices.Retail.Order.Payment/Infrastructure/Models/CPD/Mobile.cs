using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Mobile
    {

        [XmlAttribute(AttributeName = "operator-id")]
        public string OperatorId { get; set; }

        [XmlAttribute(AttributeName = "country-id")]
        public string CountryId { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
