namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:accessamerica.services.insurance.travel")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:accessamerica.services.insurance.travel", IsNullable = false)]
    public partial class SendPurchases
    {

        private Partner partnerField;

        private PurchasesRequest purchasesRequestField;

        /// <remarks/>
        public Partner Partner
        {
            get
            {
                return this.partnerField;
            }
            set
            {
                this.partnerField = value;
            }
        }

        /// <remarks/>
        public PurchasesRequest PurchasesRequest
        {
            get
            {
                return this.purchasesRequestField;
            }
            set
            {
                this.purchasesRequestField = value;
            }
        }
    }
    
}
