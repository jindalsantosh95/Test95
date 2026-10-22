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
    public class PurchasesResponsePurchasesPurchaseProductDetails
    {

        private PurchasesResponsePurchasesPurchaseProductDetailsProduct productField;

        /// <remarks/>
        public PurchasesResponsePurchasesPurchaseProductDetailsProduct Product
        {
            get
            {
                return productField;
            }
            set
            {
                productField = value;
            }
        }
    }
}
