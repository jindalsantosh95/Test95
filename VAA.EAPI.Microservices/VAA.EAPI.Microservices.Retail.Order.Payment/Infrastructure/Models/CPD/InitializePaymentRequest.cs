using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD
{
    [XmlRoot(ElementName = "root")]
    public class InitializePaymentRequest
    {
        [XmlElement(ElementName = "initialize-payment")]
        public Initializepayment Initializepayment { get; set; }

    }
}
