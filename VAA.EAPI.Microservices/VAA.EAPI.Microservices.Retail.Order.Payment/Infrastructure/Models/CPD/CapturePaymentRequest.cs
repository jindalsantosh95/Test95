using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class CapturePaymentRequest
    {
        [XmlElement(ElementName = "update-order-data")]
        public UpdateOrderData UpdateOrderData { get; set; }
    }
}
