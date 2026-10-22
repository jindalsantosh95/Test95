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
    public class PurchasesResponsePurchasesPurchaseProductDetailsProduct
    {

        private byte sequenceField;

        private string productIDField;

        private string productBookingIDField;

        /// <remarks/>
        public byte Sequence
        {
            get
            {
                return sequenceField;
            }
            set
            {
                sequenceField = value;
            }
        }

        /// <remarks/>
        public string ProductID
        {
            get
            {
                return productIDField;
            }
            set
            {
                productIDField = value;
            }
        }

        /// <remarks/>
        public string ProductBookingID
        {
            get
            {
                return productBookingIDField;
            }
            set
            {
                productBookingIDField = value;
            }
        }
    }
}
