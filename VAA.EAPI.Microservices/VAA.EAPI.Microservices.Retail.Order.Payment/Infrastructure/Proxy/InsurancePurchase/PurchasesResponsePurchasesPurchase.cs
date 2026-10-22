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
    public class PurchasesResponsePurchasesPurchase
    {

        private string partnerBookingIDField;

        private string orderIDField;

        private string statusField;

        private PurchasesResponsePurchasesPurchaseProductDetails productDetailsField;

        /// <remarks/>
        public string PartnerBookingID
        {
            get
            {
                return partnerBookingIDField;
            }
            set
            {
                partnerBookingIDField = value;
            }
        }

        /// <remarks/>
        public string OrderID
        {
            get
            {
                return orderIDField;
            }
            set
            {
                orderIDField = value;
            }
        }

        /// <remarks/>
        public string Status
        {
            get
            {
                return statusField;
            }
            set
            {
                statusField = value;
            }
        }

        /// <remarks/>
        public PurchasesResponsePurchasesPurchaseProductDetails ProductDetails
        {
            get
            {
                return productDetailsField;
            }
            set
            {
                productDetailsField = value;
            }
        }
    }

}
