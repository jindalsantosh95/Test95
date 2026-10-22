namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]    
    public partial class GadgetDetails
    {

        private string browserTypeField;

        private object browserVersionField;

        private object deviceCategoryField;

        private object deviceTypeField;

        private string operatingSystemField;

        private object operatingSystemVersionField;

        private string requestingPlatformField;

        private object requestingPlatformVersionField;

        /// <remarks/>
        public string BrowserType
        {
            get; set;
        }

        /// <remarks/>
        public object BrowserVersion
        {
            get
            {
                return this.browserVersionField;
            }
            set
            {
                this.browserVersionField = value;
            }
        }

        /// <remarks/>
        public object DeviceCategory
        {
            get
            {
                return this.deviceCategoryField;
            }
            set
            {
                this.deviceCategoryField = value;
            }
        }

        /// <remarks/>
        public object DeviceType
        {
            get
            {
                return this.deviceTypeField;
            }
            set
            {
                this.deviceTypeField = value;
            }
        }

        /// <remarks/>
        public string OperatingSystem
        {
            get
            {
                return this.operatingSystemField;
            }
            set
            {
                this.operatingSystemField = value;
            }
        }

        /// <remarks/>
        public object OperatingSystemVersion
        {
            get
            {
                return this.operatingSystemVersionField;
            }
            set
            {
                this.operatingSystemVersionField = value;
            }
        }

        /// <remarks/>
        public string RequestingPlatform
        {
            get
            {
                return this.requestingPlatformField;
            }
            set
            {
                this.requestingPlatformField = value;
            }
        }

        /// <remarks/>
        public object RequestingPlatformVersion
        {
            get
            {
                return this.requestingPlatformVersionField;
            }
            set
            {
                this.requestingPlatformVersionField = value;
            }
        }
    }

}