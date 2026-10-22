namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]    
    public partial class PurchasesRequest
    {

        private string accamField;

        private string businessUnitField;

        private string transactionIDField;

        private Purchase[] purchasesField;

        /// <remarks/>
        public string Accam
        {
            get
            {
                return this.accamField;
            }
            set
            {
                this.accamField = value;
            }
        }

        /// <remarks/>
        public string BusinessUnit
        {
            get
            {
                return this.businessUnitField;
            }
            set
            {
                this.businessUnitField = value;
            }
        }

        /// <remarks/>
        public string TransactionID
        {
            get
            {
                return this.transactionIDField;
            }
            set
            {
                this.transactionIDField = value;
            }
        }

        /// <remarks/>
        public Purchase[] Purchases
        {
            get
            {
                return this.purchasesField;
            }
            set
            {
                this.purchasesField = value;
            }
        }
    }


}