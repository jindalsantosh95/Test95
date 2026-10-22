namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TravelDetails
    {

        private string departureDateField;

        private string returnDateField;

        private OriginDestinationDetails originField;

        private OriginDestinationDetails destinationField;

        private Provider[] providersField;

        /// <remarks/>
        public string DepartureDate
        {
            get
            {
                return this.departureDateField;
            }
            set
            {
                this.departureDateField = value;
            }
        }

        /// <remarks/>
        public string ReturnDate
        {
            get
            {
                return this.returnDateField;
            }
            set
            {
                this.returnDateField = value;
            }
        }

        /// <remarks/>
        public OriginDestinationDetails Origin
        {
            get
            {
                return this.originField;
            }
            set
            {
                this.originField = value;
            }
        }

        /// <remarks/>
        public OriginDestinationDetails Destination
        {
            get
            {
                return this.destinationField;
            }
            set
            {
                this.destinationField = value;
            }
        }

        /// <remarks/>
        public Provider[] Providers
        {
            get
            {
                return this.providersField;
            }
            set
            {
                this.providersField = value;
            }
        }
    }

}