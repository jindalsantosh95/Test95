namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]   
    public partial class Product
    {

        private byte sequenceField;

        private string quoteIDField;

        private string productIDField;

        private Insured[] insuredDetailsField;

        private decimal coverageAmountField;

        private decimal priceField;

        private decimal premiumPriceField;

        private decimal charitableContributionField;

        private string startDateField;

        private string endDateField;

        private string productBookingIDField;

        /// <remarks/>
        public byte Sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }

        /// <remarks/>
        public string QuoteID
        {
            get
            {
                return this.quoteIDField;
            }
            set
            {
                this.quoteIDField = value;
            }
        }

        /// <remarks/>
        public string ProductID
        {
            get
            {
                return this.productIDField;
            }
            set
            {
                this.productIDField = value;
            }
        }

        /// <remarks/>
        public Insured[] InsuredDetails
        {
            get
            {
                return this.insuredDetailsField;
            }
            set
            {
                this.insuredDetailsField = value;
            }
        }

        /// <remarks/>
        public decimal CoverageAmount
        {
            get
            {
                return this.coverageAmountField;
            }
            set
            {
                this.coverageAmountField = value;
            }
        }

        /// <remarks/>
        public decimal Price
        {
            get
            {
                return this.priceField;
            }
            set
            {
                this.priceField = value;
            }
        }

        /// <remarks/>
        public decimal PremiumPrice
        {
            get
            {
                return this.premiumPriceField;
            }
            set
            {
                this.premiumPriceField = value;
            }
        }

        /// <remarks/>
        public decimal CharitableContribution
        {
            get
            {
                return this.charitableContributionField;
            }
            set
            {
                this.charitableContributionField = value;
            }
        }

        /// <remarks/>
        public string StartDate
        {
            get
            {
                return this.startDateField;
            }
            set
            {
                this.startDateField = value;
            }
        }

        /// <remarks/>
        public string EndDate
        {
            get
            {
                return this.endDateField;
            }
            set
            {
                this.endDateField = value;
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