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
    public class PurchasesResponsePurchases
    {

        private PurchasesResponsePurchasesPurchase purchaseField;

        /// <remarks/>
        public PurchasesResponsePurchasesPurchase Purchase
        {
            get
            {
                return purchaseField;
            }
            set
            {
                purchaseField = value;
            }
        }
    }

}
