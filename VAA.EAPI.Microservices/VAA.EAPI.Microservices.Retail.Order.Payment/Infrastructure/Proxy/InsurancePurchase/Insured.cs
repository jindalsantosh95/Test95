namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
  
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]    
    public partial class Insured
    {

        private byte insuredSequenceField;

        private string insuredTypeField;

        private string firstNameField;

        private string lastNameField;

        private string birthdateField;

        private string phoneNumberField;

        private bool phoneNumberFieldSpecified;

        private decimal tripCostField;

        /// <remarks/>
        public byte InsuredSequence
        {
            get
            {
                return this.insuredSequenceField;
            }
            set
            {
                this.insuredSequenceField = value;
            }
        }

        /// <remarks/>
        public string InsuredType
        {
            get
            {
                return this.insuredTypeField;
            }
            set
            {
                this.insuredTypeField = value;
            }
        }

        /// <remarks/>
        public string FirstName
        {
            get
            {
                return this.firstNameField;
            }
            set
            {
                this.firstNameField = value;
            }
        }

        /// <remarks/>
        public string LastName
        {
            get
            {
                return this.lastNameField;
            }
            set
            {
                this.lastNameField = value;
            }
        }

        /// <remarks/>
        public string Birthdate
        {
            get
            {
                return this.birthdateField;
            }
            set
            {
                this.birthdateField = value;
            }
        }

        /// <remarks/>
        public string PhoneNumber
        {
            get
            {
                return this.phoneNumberField;
            }
            set
            {
                this.phoneNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PhoneNumberSpecified
        {
            get
            {
                return this.phoneNumberFieldSpecified;
            }
            set
            {
                this.phoneNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal TripCost
        {
            get
            {
                return this.tripCostField;
            }
            set
            {
                this.tripCostField = value;
            }
        }
    }


}
