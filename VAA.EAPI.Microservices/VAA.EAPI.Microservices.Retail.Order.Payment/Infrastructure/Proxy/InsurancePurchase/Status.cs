using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "urn:accessamerica.services.insurance.travel")]
    public class Status
    {

        private string codeField;

        private string messageField;

        /// <remarks/>
        public string Code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
            }
        }

        /// <remarks/>
        public string Message
        {
            get
            {
                return messageField;
            }
            set
            {
                messageField = value;
            }
        }
    }
}
