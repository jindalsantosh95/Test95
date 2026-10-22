namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRoot(Namespace = "", IsNullable = false)]
    public class SendPurchasesResponse
    {

        private PurchasesResponse purchasesResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "urn:accessamerica.services.insurance.travel")]
        public PurchasesResponse PurchasesResponse
        {
            get
            {
                return purchasesResponseField;
            }
            set
            {
                purchasesResponseField = value;
            }
        }
    } 
}