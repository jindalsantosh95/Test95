using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    public class Orders
    {
        [XmlElement(ElementName = "line-item")]
        public List<LineItem> LineItem { get; set; }
    }
}
