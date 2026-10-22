using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Cards
    {
        [XmlElement(ElementName = "card")]
        public List<Card> Card { get; set; }
    }
}
