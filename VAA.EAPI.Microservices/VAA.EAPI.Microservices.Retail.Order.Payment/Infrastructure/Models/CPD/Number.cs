using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Number
    {
        [XmlText]
        public string Text { get; set; }
    }
}