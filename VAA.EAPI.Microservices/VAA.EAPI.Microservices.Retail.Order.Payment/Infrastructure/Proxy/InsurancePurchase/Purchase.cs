namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]    
    public partial class Purchase
    {

        private string partnerBookingIDField;

        private InsuranceBookingDetails insuranceBookingDetailsField;

        private GadgetDetails gadgetDetailsField;

        private TravelBookingDetails travelBookingDetailsField;

        private TravelDetails travelDetailsField;

        private Product[] productDetailsField;

        /// <remarks/>
        public string PartnerBookingID
        {
            get
            {
                return this.partnerBookingIDField;
            }
            set
            {
                this.partnerBookingIDField = value;
            }
        }

        /// <remarks/>
        public InsuranceBookingDetails InsuranceBookingDetails
        {
            get
            {
                return this.insuranceBookingDetailsField;
            }
            set
            {
                this.insuranceBookingDetailsField = value;
            }
        }

        /// <remarks/>
        public GadgetDetails GadgetDetails
        {
            get
            {
                return this.gadgetDetailsField;
            }
            set
            {
                this.gadgetDetailsField = value;
            }
        }

        /// <remarks/>
        public TravelBookingDetails TravelBookingDetails
        {
            get
            {
                return this.travelBookingDetailsField;
            }
            set
            {
                this.travelBookingDetailsField = value;
            }
        }

        /// <remarks/>
        public TravelDetails TravelDetails
        {
            get
            {
                return this.travelDetailsField;
            }
            set
            {
                this.travelDetailsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Product", IsNullable = false)]
        public Product[] ProductDetails
        {
            get
            {
                return this.productDetailsField;
            }
            set
            {
                this.productDetailsField = value;
            }
        }
    }


}