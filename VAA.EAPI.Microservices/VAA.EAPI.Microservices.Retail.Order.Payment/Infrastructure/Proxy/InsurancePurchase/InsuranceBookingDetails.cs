namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]    
    public partial class InsuranceBookingDetails
    {

        private System.DateTime purchaseDateTimeField;

        private string methodField;

        private string statusField;

        private string typeField;

        private string currencyField;

        private decimal totalCoverageAmountField;

        private decimal totalPriceField;

        private decimal totalPremiumPriceField;

        private decimal waiverPriceField;

        private decimal streetRateField;

        private InsuranceBookingDetailsPurchaser purchaserField;

        private string fulfillmentMethodField;

        private string paymentMethodField;

        /// <remarks/>
        public System.DateTime PurchaseDateTime
        {
            get
            {
                return this.purchaseDateTimeField;
            }
            set
            {
                this.purchaseDateTimeField = value;
            }
        }

        /// <remarks/>
        public string Method
        {
            get
            {
                return this.methodField;
            }
            set
            {
                this.methodField = value;
            }
        }

        /// <remarks/>
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        public string Currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        /// <remarks/>
        public decimal TotalCoverageAmount
        {
            get
            {
                return this.totalCoverageAmountField;
            }
            set
            {
                this.totalCoverageAmountField = value;
            }
        }

        /// <remarks/>
        public decimal TotalPrice
        {
            get
            {
                return this.totalPriceField;
            }
            set
            {
                this.totalPriceField = value;
            }
        }

        /// <remarks/>
        public decimal TotalPremiumPrice
        {
            get
            {
                return this.totalPremiumPriceField;
            }
            set
            {
                this.totalPremiumPriceField = value;
            }
        }

        /// <remarks/>
        public decimal WaiverPrice
        {
            get
            {
                return this.waiverPriceField;
            }
            set
            {
                this.waiverPriceField = value;
            }
        }

        /// <remarks/>
        public decimal StreetRate
        {
            get
            {
                return this.streetRateField;
            }
            set
            {
                this.streetRateField = value;
            }
        }

        /// <remarks/>
        public InsuranceBookingDetailsPurchaser Purchaser
        {
            get
            {
                return this.purchaserField;
            }
            set
            {
                this.purchaserField = value;
            }
        }

        /// <remarks/>
        public string FulfillmentMethod
        {
            get
            {
                return this.fulfillmentMethodField;
            }
            set
            {
                this.fulfillmentMethodField = value;
            }
        }

        /// <remarks/>
        public string PaymentMethod
        {
            get
            {
                return this.paymentMethodField;
            }
            set
            {
                this.paymentMethodField = value;
            }
        }
    }

}