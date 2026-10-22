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
    [System.Xml.Serialization.XmlRoot(Namespace = "urn:accessamerica.services.insurance.travel", IsNullable = false)]
    public class PurchasesResponse
    {

        private string transactionIDField;

        private Status statusField;

        private PurchasesResponsePurchases purchasesField;

        /// <remarks/>
        public string TransactionID
        {
            get
            {
                return transactionIDField;
            }
            set
            {
                transactionIDField = value;
            }
        }

        /// <remarks/>
        public Status Status
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
        public PurchasesResponsePurchases Purchases
        {
            get
            {
                return purchasesField;
            }
            set
            {
                purchasesField = value;
            }
        }
    }
}
