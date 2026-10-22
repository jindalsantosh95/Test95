using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Code
    {              
        [XmlText]
        public string Text { get; set; }
    }
}
