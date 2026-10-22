namespace VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage", IsNullable = false)]
    public partial class IATA_OfferPriceRQ
    {

        private AugmentationPointType augmentationPointField;

        private DistributionChainLinkType[] distributionChainField;

        private IATA_PayloadStandardAttributesType payloadAttributesField;

        private POS_Type pOSField;

        private OfferPriceRequestType requestField;

        /// <remarks/>
        public AugmentationPointType AugmentationPoint
        {
            get
            {
                return this.augmentationPointField;
            }
            set
            {
                this.augmentationPointField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DistributionChainLink", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes", IsNullable = false)]
        public DistributionChainLinkType[] DistributionChain
        {
            get
            {
                return this.distributionChainField;
            }
            set
            {
                this.distributionChainField = value;
            }
        }

        /// <remarks/>
        public IATA_PayloadStandardAttributesType PayloadAttributes
        {
            get
            {
                return this.payloadAttributesField;
            }
            set
            {
                this.payloadAttributesField = value;
            }
        }

        /// <remarks/>
        public POS_Type POS
        {
            get
            {
                return this.pOSField;
            }
            set
            {
                this.pOSField = value;
            }
        }

        /// <remarks/>
        public OfferPriceRequestType Request
        {
            get
            {
                return this.requestField;
            }
            set
            {
                this.requestField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AugmentationPointType
    {

        private System.Xml.XmlElement[] anyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlElement[] Any
        {
            get
            {
                return this.anyField;
            }
            set
            {
                this.anyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PricingParameterType
    {

        private bool autoExchIndField;

        private bool autoExchIndFieldSpecified;

        private bool awardIncludedIndField;

        private bool awardIncludedIndFieldSpecified;

        private bool awardOnlyIndField;

        private bool awardOnlyIndFieldSpecified;

        private bool includeUpsellOffersIndField;

        private bool includeUpsellOffersIndFieldSpecified;

        private string overrideCurCodeField;

        private bool simplePricingIndField;

        private bool simplePricingIndFieldSpecified;

        private TaxSummaryType[] taxExemptionField;

        /// <remarks/>
        public bool AutoExchInd
        {
            get
            {
                return this.autoExchIndField;
            }
            set
            {
                this.autoExchIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AutoExchIndSpecified
        {
            get
            {
                return this.autoExchIndFieldSpecified;
            }
            set
            {
                this.autoExchIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool AwardIncludedInd
        {
            get
            {
                return this.awardIncludedIndField;
            }
            set
            {
                this.awardIncludedIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AwardIncludedIndSpecified
        {
            get
            {
                return this.awardIncludedIndFieldSpecified;
            }
            set
            {
                this.awardIncludedIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool AwardOnlyInd
        {
            get
            {
                return this.awardOnlyIndField;
            }
            set
            {
                this.awardOnlyIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AwardOnlyIndSpecified
        {
            get
            {
                return this.awardOnlyIndFieldSpecified;
            }
            set
            {
                this.awardOnlyIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool IncludeUpsellOffersInd
        {
            get
            {
                return this.includeUpsellOffersIndField;
            }
            set
            {
                this.includeUpsellOffersIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IncludeUpsellOffersIndSpecified
        {
            get
            {
                return this.includeUpsellOffersIndFieldSpecified;
            }
            set
            {
                this.includeUpsellOffersIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OverrideCurCode
        {
            get
            {
                return this.overrideCurCodeField;
            }
            set
            {
                this.overrideCurCodeField = value;
            }
        }

        /// <remarks/>
        public bool SimplePricingInd
        {
            get
            {
                return this.simplePricingIndField;
            }
            set
            {
                this.simplePricingIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SimplePricingIndSpecified
        {
            get
            {
                return this.simplePricingIndFieldSpecified;
            }
            set
            {
                this.simplePricingIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TaxExemption")]
        public TaxSummaryType[] TaxExemption
        {
            get
            {
                return this.taxExemptionField;
            }
            set
            {
                this.taxExemptionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TaxSummaryType
    {

        private bool allRefundableIndField;

        private bool allRefundableIndFieldSpecified;

        private bool approximateIndField;

        private bool approximateIndFieldSpecified;

        private bool collectionIndField;

        private bool collectionIndFieldSpecified;

        private System.DateTime guaranteeTimeLimitDateTimeField;

        private bool guaranteeTimeLimitDateTimeFieldSpecified;

        private string refundMethodTextField;

        private TaxType[] taxField;

        private AmountType totalRefundableTaxAmountField;

        private AmountType totalTaxAmountField;

        /// <remarks/>
        public bool AllRefundableInd
        {
            get
            {
                return this.allRefundableIndField;
            }
            set
            {
                this.allRefundableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllRefundableIndSpecified
        {
            get
            {
                return this.allRefundableIndFieldSpecified;
            }
            set
            {
                this.allRefundableIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return this.approximateIndField;
            }
            set
            {
                this.approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return this.approximateIndFieldSpecified;
            }
            set
            {
                this.approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool CollectionInd
        {
            get
            {
                return this.collectionIndField;
            }
            set
            {
                this.collectionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CollectionIndSpecified
        {
            get
            {
                return this.collectionIndFieldSpecified;
            }
            set
            {
                this.collectionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime GuaranteeTimeLimitDateTime
        {
            get
            {
                return this.guaranteeTimeLimitDateTimeField;
            }
            set
            {
                this.guaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return this.guaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                this.guaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string RefundMethodText
        {
            get
            {
                return this.refundMethodTextField;
            }
            set
            {
                this.refundMethodTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Tax")]
        public TaxType[] Tax
        {
            get
            {
                return this.taxField;
            }
            set
            {
                this.taxField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalRefundableTaxAmount
        {
            get
            {
                return this.totalRefundableTaxAmountField;
            }
            set
            {
                this.totalRefundableTaxAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalTaxAmount
        {
            get
            {
                return this.totalTaxAmountField;
            }
            set
            {
                this.totalTaxAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TaxType
    {

        private string addlFiledTaxCodeField;

        private string addlTaxCodeField;

        private AmountType amountField;

        private bool approximateIndField;

        private bool approximateIndFieldSpecified;

        private string appTypeCodeField;

        private bool collectionIndField;

        private bool collectionIndFieldSpecified;

        private CollectionPointTaxType[] collectionPointTaxField;

        private CountryType countryField;

        private CountrySubDivisionType countrySubDivisionField;

        private CurConversionType curConversionField;

        private string descTextField;

        private AmountType filedAmountField;

        private string filedTaxCodeField;

        private string qualifierCodeField;

        private bool refundIndField;

        private bool refundIndFieldSpecified;

        private string taxCodeField;

        private string taxNameField;

        private TaxTypeCodeType taxTypeCodeField;

        private bool taxTypeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AddlFiledTaxCode
        {
            get
            {
                return this.addlFiledTaxCodeField;
            }
            set
            {
                this.addlFiledTaxCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AddlTaxCode
        {
            get
            {
                return this.addlTaxCodeField;
            }
            set
            {
                this.addlTaxCodeField = value;
            }
        }

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return this.approximateIndField;
            }
            set
            {
                this.approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return this.approximateIndFieldSpecified;
            }
            set
            {
                this.approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AppTypeCode
        {
            get
            {
                return this.appTypeCodeField;
            }
            set
            {
                this.appTypeCodeField = value;
            }
        }

        /// <remarks/>
        public bool CollectionInd
        {
            get
            {
                return this.collectionIndField;
            }
            set
            {
                this.collectionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CollectionIndSpecified
        {
            get
            {
                return this.collectionIndFieldSpecified;
            }
            set
            {
                this.collectionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CollectionPointTax")]
        public CollectionPointTaxType[] CollectionPointTax
        {
            get
            {
                return this.collectionPointTaxField;
            }
            set
            {
                this.collectionPointTaxField = value;
            }
        }

        /// <remarks/>
        public CountryType Country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        public CountrySubDivisionType CountrySubDivision
        {
            get
            {
                return this.countrySubDivisionField;
            }
            set
            {
                this.countrySubDivisionField = value;
            }
        }

        /// <remarks/>
        public CurConversionType CurConversion
        {
            get
            {
                return this.curConversionField;
            }
            set
            {
                this.curConversionField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType FiledAmount
        {
            get
            {
                return this.filedAmountField;
            }
            set
            {
                this.filedAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FiledTaxCode
        {
            get
            {
                return this.filedTaxCodeField;
            }
            set
            {
                this.filedTaxCodeField = value;
            }
        }

        /// <remarks/>
        public string QualifierCode
        {
            get
            {
                return this.qualifierCodeField;
            }
            set
            {
                this.qualifierCodeField = value;
            }
        }

        /// <remarks/>
        public bool RefundInd
        {
            get
            {
                return this.refundIndField;
            }
            set
            {
                this.refundIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RefundIndSpecified
        {
            get
            {
                return this.refundIndFieldSpecified;
            }
            set
            {
                this.refundIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TaxCode
        {
            get
            {
                return this.taxCodeField;
            }
            set
            {
                this.taxCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TaxName
        {
            get
            {
                return this.taxNameField;
            }
            set
            {
                this.taxNameField = value;
            }
        }

        /// <remarks/>
        public TaxTypeCodeType TaxTypeCode
        {
            get
            {
                return this.taxTypeCodeField;
            }
            set
            {
                this.taxTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TaxTypeCodeSpecified
        {
            get
            {
                return this.taxTypeCodeFieldSpecified;
            }
            set
            {
                this.taxTypeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AmountType
    {

        private string curCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
        public string CurCode
        {
            get
            {
                return this.curCodeField;
            }
            set
            {
                this.curCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CollectionPointTaxType
    {

        private AmountType airportAmountField;

        private StationType stationField;

        /// <remarks/>
        public AmountType AirportAmount
        {
            get
            {
                return this.airportAmountField;
            }
            set
            {
                this.airportAmountField = value;
            }
        }

        /// <remarks/>
        public StationType Station
        {
            get
            {
                return this.stationField;
            }
            set
            {
                this.stationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class StationType
    {

        private string iATA_LocationCodeField;

        private string stationNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IATA_LocationCode
        {
            get
            {
                return this.iATA_LocationCodeField;
            }
            set
            {
                this.iATA_LocationCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string StationName
        {
            get
            {
                return this.stationNameField;
            }
            set
            {
                this.stationNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CountryType
    {

        private string countryCodeField;

        private string countryNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return this.countryCodeField;
            }
            set
            {
                this.countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryName
        {
            get
            {
                return this.countryNameField;
            }
            set
            {
                this.countryNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CountrySubDivisionType
    {

        private string countrySubDivisionCodeField;

        private string countrySubDivisionNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountrySubDivisionCode
        {
            get
            {
                return this.countrySubDivisionCodeField;
            }
            set
            {
                this.countrySubDivisionCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountrySubDivisionName
        {
            get
            {
                return this.countrySubDivisionNameField;
            }
            set
            {
                this.countrySubDivisionNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CurConversionType
    {

        private AmountType amountField;

        private RateType conversionRateField;

        private AmountType localAmountField;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        public RateType ConversionRate
        {
            get
            {
                return this.conversionRateField;
            }
            set
            {
                this.conversionRateField = value;
            }
        }

        /// <remarks/>
        public AmountType LocalAmount
        {
            get
            {
                return this.localAmountField;
            }
            set
            {
                this.localAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RateType
    {

        private string baseCurCodeField;

        private decimal baseMultiplierValueField;

        private bool baseMultiplierValueFieldSpecified;

        private string baseUnitCodeField;

        private string curCodeField;

        private string formatField;

        private decimal multiplierValueField;

        private bool multiplierValueFieldSpecified;

        private string unitCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
        public string BaseCurCode
        {
            get
            {
                return this.baseCurCodeField;
            }
            set
            {
                this.baseCurCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal BaseMultiplierValue
        {
            get
            {
                return this.baseMultiplierValueField;
            }
            set
            {
                this.baseMultiplierValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BaseMultiplierValueSpecified
        {
            get
            {
                return this.baseMultiplierValueFieldSpecified;
            }
            set
            {
                this.baseMultiplierValueFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
        public string BaseUnitCode
        {
            get
            {
                return this.baseUnitCodeField;
            }
            set
            {
                this.baseUnitCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
        public string CurCode
        {
            get
            {
                return this.curCodeField;
            }
            set
            {
                this.curCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal MultiplierValue
        {
            get
            {
                return this.multiplierValueField;
            }
            set
            {
                this.multiplierValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MultiplierValueSpecified
        {
            get
            {
                return this.multiplierValueFieldSpecified;
            }
            set
            {
                this.multiplierValueFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
        public string UnitCode
        {
            get
            {
                return this.unitCodeField;
            }
            set
            {
                this.unitCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TaxTypeCodeType
    {

        /// <remarks/>
        Applied,

        /// <remarks/>
        Exempt,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxGroupType
    {

        private ContactInfoType[] contactInfoField;

        private decimal intendedPaxQtyField;

        private bool intendedPaxQtyFieldSpecified;

        private string paxGroupIDField;

        private string paxGroupNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ContactInfo")]
        public ContactInfoType[] ContactInfo
        {
            get
            {
                return this.contactInfoField;
            }
            set
            {
                this.contactInfoField = value;
            }
        }

        /// <remarks/>
        public decimal IntendedPaxQty
        {
            get
            {
                return this.intendedPaxQtyField;
            }
            set
            {
                this.intendedPaxQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IntendedPaxQtySpecified
        {
            get
            {
                return this.intendedPaxQtyFieldSpecified;
            }
            set
            {
                this.intendedPaxQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxGroupID
        {
            get
            {
                return this.paxGroupIDField;
            }
            set
            {
                this.paxGroupIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxGroupName
        {
            get
            {
                return this.paxGroupNameField;
            }
            set
            {
                this.paxGroupNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ContactInfoType
    {

        private string contactInfoIDField;

        private string[] contactPurposeTextField;

        private bool contactRefusedIndField;

        private bool contactRefusedIndFieldSpecified;

        private EmailAddressType[] emailAddressField;

        private IndividualType individualField;

        private string individualRefIDField;

        private OtherAddressType[] otherAddressField;

        private string paxSegmentRefIDField;

        private PhoneType[] phoneField;

        private PostalAddressType[] postalAddressField;

        private string relationshipToPaxField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ContactInfoID
        {
            get
            {
                return this.contactInfoIDField;
            }
            set
            {
                this.contactInfoIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ContactPurposeText")]
        public string[] ContactPurposeText
        {
            get
            {
                return this.contactPurposeTextField;
            }
            set
            {
                this.contactPurposeTextField = value;
            }
        }

        /// <remarks/>
        public bool ContactRefusedInd
        {
            get
            {
                return this.contactRefusedIndField;
            }
            set
            {
                this.contactRefusedIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ContactRefusedIndSpecified
        {
            get
            {
                return this.contactRefusedIndFieldSpecified;
            }
            set
            {
                this.contactRefusedIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("EmailAddress")]
        public EmailAddressType[] EmailAddress
        {
            get
            {
                return this.emailAddressField;
            }
            set
            {
                this.emailAddressField = value;
            }
        }

        /// <remarks/>
        public IndividualType Individual
        {
            get
            {
                return this.individualField;
            }
            set
            {
                this.individualField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IndividualRefID
        {
            get
            {
                return this.individualRefIDField;
            }
            set
            {
                this.individualRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OtherAddress")]
        public OtherAddressType[] OtherAddress
        {
            get
            {
                return this.otherAddressField;
            }
            set
            {
                this.otherAddressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxSegmentRefID
        {
            get
            {
                return this.paxSegmentRefIDField;
            }
            set
            {
                this.paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Phone")]
        public PhoneType[] Phone
        {
            get
            {
                return this.phoneField;
            }
            set
            {
                this.phoneField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PostalAddress")]
        public PostalAddressType[] PostalAddress
        {
            get
            {
                return this.postalAddressField;
            }
            set
            {
                this.postalAddressField = value;
            }
        }

        /// <remarks/>
        public string RelationshipToPax
        {
            get
            {
                return this.relationshipToPaxField;
            }
            set
            {
                this.relationshipToPaxField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class EmailAddressType
    {

        private string contactTypeTextField;

        private string emailAddressTextField;

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return this.contactTypeTextField;
            }
            set
            {
                this.contactTypeTextField = value;
            }
        }

        /// <remarks/>
        public string EmailAddressText
        {
            get
            {
                return this.emailAddressTextField;
            }
            set
            {
                this.emailAddressTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IndividualType
    {

        private AddlNameType[] addlNameField;

        private System.DateTime birthdateField;

        private bool birthdateFieldSpecified;

        private string birthplaceTextField;

        private GenderCodeType genderCodeField;

        private bool genderCodeFieldSpecified;

        private string[] givenNameField;

        private string individualIDField;

        private string[] middleNameField;

        private string suffixNameField;

        private string surnameField;

        private string titleNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AddlName")]
        public AddlNameType[] AddlName
        {
            get
            {
                return this.addlNameField;
            }
            set
            {
                this.addlNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime Birthdate
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BirthdateSpecified
        {
            get
            {
                return this.birthdateFieldSpecified;
            }
            set
            {
                this.birthdateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string BirthplaceText
        {
            get
            {
                return this.birthplaceTextField;
            }
            set
            {
                this.birthplaceTextField = value;
            }
        }

        /// <remarks/>
        public GenderCodeType GenderCode
        {
            get
            {
                return this.genderCodeField;
            }
            set
            {
                this.genderCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GenderCodeSpecified
        {
            get
            {
                return this.genderCodeFieldSpecified;
            }
            set
            {
                this.genderCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("GivenName", DataType = "token")]
        public string[] GivenName
        {
            get
            {
                return this.givenNameField;
            }
            set
            {
                this.givenNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IndividualID
        {
            get
            {
                return this.individualIDField;
            }
            set
            {
                this.individualIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MiddleName", DataType = "token")]
        public string[] MiddleName
        {
            get
            {
                return this.middleNameField;
            }
            set
            {
                this.middleNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SuffixName
        {
            get
            {
                return this.suffixNameField;
            }
            set
            {
                this.suffixNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Surname
        {
            get
            {
                return this.surnameField;
            }
            set
            {
                this.surnameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TitleName
        {
            get
            {
                return this.titleNameField;
            }
            set
            {
                this.titleNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AddlNameType
    {

        private string nameField;

        private AddlNameTypeCodeType nameTypeCodeField;

        private bool nameTypeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public AddlNameTypeCodeType NameTypeCode
        {
            get
            {
                return this.nameTypeCodeField;
            }
            set
            {
                this.nameTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NameTypeCodeSpecified
        {
            get
            {
                return this.nameTypeCodeFieldSpecified;
            }
            set
            {
                this.nameTypeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum AddlNameTypeCodeType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Father Surname")]
        FatherSurname,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Mother Surname")]
        MotherSurname,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum GenderCodeType
    {

        /// <remarks/>
        F,

        /// <remarks/>
        M,

        /// <remarks/>
        U,

        /// <remarks/>
        X,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OtherAddressType
    {

        private string contactTypeTextField;

        private string otherAddressTextField;

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return this.contactTypeTextField;
            }
            set
            {
                this.contactTypeTextField = value;
            }
        }

        /// <remarks/>
        public string OtherAddressText
        {
            get
            {
                return this.otherAddressTextField;
            }
            set
            {
                this.otherAddressTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PhoneType
    {

        private string areaCodeNumberField;

        private string contactTypeTextField;

        private string countryDialingCodeField;

        private string extensionNumberField;

        private string phoneNumberField;

        /// <remarks/>
        public string AreaCodeNumber
        {
            get
            {
                return this.areaCodeNumberField;
            }
            set
            {
                this.areaCodeNumberField = value;
            }
        }

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return this.contactTypeTextField;
            }
            set
            {
                this.contactTypeTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryDialingCode
        {
            get
            {
                return this.countryDialingCodeField;
            }
            set
            {
                this.countryDialingCodeField = value;
            }
        }

        /// <remarks/>
        public string ExtensionNumber
        {
            get
            {
                return this.extensionNumberField;
            }
            set
            {
                this.extensionNumberField = value;
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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PostalAddressType
    {

        private string buildingRoomTextField;

        private string cityNameField;

        private string contactTypeTextField;

        private string countryCodeField;

        private string countryNameField;

        private string countrySubDivisionNameField;

        private string pO_BoxCodeField;

        private string postalCodeField;

        private string[] streetTextField;

        /// <remarks/>
        public string BuildingRoomText
        {
            get
            {
                return this.buildingRoomTextField;
            }
            set
            {
                this.buildingRoomTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CityName
        {
            get
            {
                return this.cityNameField;
            }
            set
            {
                this.cityNameField = value;
            }
        }

        /// <remarks/>
        public string ContactTypeText
        {
            get
            {
                return this.contactTypeTextField;
            }
            set
            {
                this.contactTypeTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return this.countryCodeField;
            }
            set
            {
                this.countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryName
        {
            get
            {
                return this.countryNameField;
            }
            set
            {
                this.countryNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountrySubDivisionName
        {
            get
            {
                return this.countrySubDivisionNameField;
            }
            set
            {
                this.countrySubDivisionNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PO_BoxCode
        {
            get
            {
                return this.pO_BoxCodeField;
            }
            set
            {
                this.pO_BoxCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PostalCode
        {
            get
            {
                return this.postalCodeField;
            }
            set
            {
                this.postalCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StreetText")]
        public string[] StreetText
        {
            get
            {
                return this.streetTextField;
            }
            set
            {
                this.streetTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class GeospatialLocationType
    {

        private string naturalAreaGeocodeField;

        private decimal pointElevationNumberField;

        private bool pointElevationNumberFieldSpecified;

        private decimal pointLatitudeNumberField;

        private bool pointLatitudeNumberFieldSpecified;

        private decimal pointLongitudeNumberField;

        private bool pointLongitudeNumberFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string NaturalAreaGeocode
        {
            get
            {
                return this.naturalAreaGeocodeField;
            }
            set
            {
                this.naturalAreaGeocodeField = value;
            }
        }

        /// <remarks/>
        public decimal PointElevationNumber
        {
            get
            {
                return this.pointElevationNumberField;
            }
            set
            {
                this.pointElevationNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PointElevationNumberSpecified
        {
            get
            {
                return this.pointElevationNumberFieldSpecified;
            }
            set
            {
                this.pointElevationNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal PointLatitudeNumber
        {
            get
            {
                return this.pointLatitudeNumberField;
            }
            set
            {
                this.pointLatitudeNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PointLatitudeNumberSpecified
        {
            get
            {
                return this.pointLatitudeNumberFieldSpecified;
            }
            set
            {
                this.pointLatitudeNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal PointLongitudeNumber
        {
            get
            {
                return this.pointLongitudeNumberField;
            }
            set
            {
                this.pointLongitudeNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PointLongitudeNumberSpecified
        {
            get
            {
                return this.pointLongitudeNumberFieldSpecified;
            }
            set
            {
                this.pointLongitudeNumberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DeviceLocationType
    {

        private CityType cityField;

        private CountryType countryField;

        private GeospatialLocationType geospatialLocationField;

        /// <remarks/>
        public CityType City
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
            }
        }

        /// <remarks/>
        public CountryType Country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        public GeospatialLocationType GeospatialLocation
        {
            get
            {
                return this.geospatialLocationField;
            }
            set
            {
                this.geospatialLocationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CityType
    {

        private string cityNameField;

        private string iATA_LocationCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CityName
        {
            get
            {
                return this.cityNameField;
            }
            set
            {
                this.cityNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IATA_LocationCode
        {
            get
            {
                return this.iATA_LocationCodeField;
            }
            set
            {
                this.iATA_LocationCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DeviceType
    {

        private string deviceCodeField;

        private OwnershipTypeCodeType deviceOwnerTypeCodeField;

        private bool deviceOwnerTypeCodeFieldSpecified;

        private PresenceTypeCodeType presenceTypeCodeField;

        private bool presenceTypeCodeFieldSpecified;

        /// <remarks/>
        public string DeviceCode
        {
            get
            {
                return this.deviceCodeField;
            }
            set
            {
                this.deviceCodeField = value;
            }
        }

        /// <remarks/>
        public OwnershipTypeCodeType DeviceOwnerTypeCode
        {
            get
            {
                return this.deviceOwnerTypeCodeField;
            }
            set
            {
                this.deviceOwnerTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DeviceOwnerTypeCodeSpecified
        {
            get
            {
                return this.deviceOwnerTypeCodeFieldSpecified;
            }
            set
            {
                this.deviceOwnerTypeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PresenceTypeCodeType PresenceTypeCode
        {
            get
            {
                return this.presenceTypeCodeField;
            }
            set
            {
                this.presenceTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PresenceTypeCodeSpecified
        {
            get
            {
                return this.presenceTypeCodeFieldSpecified;
            }
            set
            {
                this.presenceTypeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum OwnershipTypeCodeType
    {

        /// <remarks/>
        Customer,

        /// <remarks/>
        Seller,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum PresenceTypeCodeType
    {

        /// <remarks/>
        CN,

        /// <remarks/>
        CP,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CurParameterType
    {

        private CurAppCodeType appCodeField;

        private bool appCodeFieldSpecified;

        private string curCodeField;

        private decimal decimalsAllowedNumberField;

        private bool decimalsAllowedNumberFieldSpecified;

        private string nameField;

        /// <remarks/>
        public CurAppCodeType AppCode
        {
            get
            {
                return this.appCodeField;
            }
            set
            {
                this.appCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AppCodeSpecified
        {
            get
            {
                return this.appCodeFieldSpecified;
            }
            set
            {
                this.appCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CurCode
        {
            get
            {
                return this.curCodeField;
            }
            set
            {
                this.curCodeField = value;
            }
        }

        /// <remarks/>
        public decimal DecimalsAllowedNumber
        {
            get
            {
                return this.decimalsAllowedNumberField;
            }
            set
            {
                this.decimalsAllowedNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DecimalsAllowedNumberSpecified
        {
            get
            {
                return this.decimalsAllowedNumberFieldSpecified;
            }
            set
            {
                this.decimalsAllowedNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum CurAppCodeType
    {

        /// <remarks/>
        Actual,

        /// <remarks/>
        Conversion,

        /// <remarks/>
        Display,

        /// <remarks/>
        Other,

        /// <remarks/>
        Requested,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ResponseParametersRequestType
    {

        private BDC_Type bDCField;

        private CurParameterType[] curParameterField;

        private DeviceType deviceField;

        private DeviceLocationType deviceLocationField;

        private bool inventoryGuaranteeIndField;

        private bool inventoryGuaranteeIndFieldSpecified;

        private LangUsageType[] langUsageField;

        private MeasurementSystemCodeType measurementSystemCodeField;

        private bool measurementSystemCodeFieldSpecified;

        private PaxGroupType paxGroupField;

        private PricingParameterType pricingParameterField;

        private string reasonCodeField;

        private RemarkType[] remarkField;

        /// <remarks/>
        public BDC_Type BDC
        {
            get
            {
                return this.bDCField;
            }
            set
            {
                this.bDCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CurParameter")]
        public CurParameterType[] CurParameter
        {
            get
            {
                return this.curParameterField;
            }
            set
            {
                this.curParameterField = value;
            }
        }

        /// <remarks/>
        public DeviceType Device
        {
            get
            {
                return this.deviceField;
            }
            set
            {
                this.deviceField = value;
            }
        }

        /// <remarks/>
        public DeviceLocationType DeviceLocation
        {
            get
            {
                return this.deviceLocationField;
            }
            set
            {
                this.deviceLocationField = value;
            }
        }

        /// <remarks/>
        public bool InventoryGuaranteeInd
        {
            get
            {
                return this.inventoryGuaranteeIndField;
            }
            set
            {
                this.inventoryGuaranteeIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool InventoryGuaranteeIndSpecified
        {
            get
            {
                return this.inventoryGuaranteeIndFieldSpecified;
            }
            set
            {
                this.inventoryGuaranteeIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LangUsage")]
        public LangUsageType[] LangUsage
        {
            get
            {
                return this.langUsageField;
            }
            set
            {
                this.langUsageField = value;
            }
        }

        /// <remarks/>
        public MeasurementSystemCodeType MeasurementSystemCode
        {
            get
            {
                return this.measurementSystemCodeField;
            }
            set
            {
                this.measurementSystemCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MeasurementSystemCodeSpecified
        {
            get
            {
                return this.measurementSystemCodeFieldSpecified;
            }
            set
            {
                this.measurementSystemCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PaxGroupType PaxGroup
        {
            get
            {
                return this.paxGroupField;
            }
            set
            {
                this.paxGroupField = value;
            }
        }

        /// <remarks/>
        public PricingParameterType PricingParameter
        {
            get
            {
                return this.pricingParameterField;
            }
            set
            {
                this.pricingParameterField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ReasonCode
        {
            get
            {
                return this.reasonCodeField;
            }
            set
            {
                this.reasonCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return this.remarkField;
            }
            set
            {
                this.remarkField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BDC_Type
    {

        private BagRuleCodeType bagRuleCodeField;

        private bool bagRuleCodeFieldSpecified;

        private string bDC_AnalysisResultCodeField;

        private string bDC_ReasonTextField;

        private string carrierDesigCodeField;

        private string carrierNameField;

        /// <remarks/>
        public BagRuleCodeType BagRuleCode
        {
            get
            {
                return this.bagRuleCodeField;
            }
            set
            {
                this.bagRuleCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BagRuleCodeSpecified
        {
            get
            {
                return this.bagRuleCodeFieldSpecified;
            }
            set
            {
                this.bagRuleCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string BDC_AnalysisResultCode
        {
            get
            {
                return this.bDC_AnalysisResultCodeField;
            }
            set
            {
                this.bDC_AnalysisResultCodeField = value;
            }
        }

        /// <remarks/>
        public string BDC_ReasonText
        {
            get
            {
                return this.bDC_ReasonTextField;
            }
            set
            {
                this.bDC_ReasonTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CarrierDesigCode
        {
            get
            {
                return this.carrierDesigCodeField;
            }
            set
            {
                this.carrierDesigCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CarrierName
        {
            get
            {
                return this.carrierNameField;
            }
            set
            {
                this.carrierNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum BagRuleCodeType
    {

        /// <remarks/>
        D,

        /// <remarks/>
        N,

        /// <remarks/>
        Other,

        /// <remarks/>
        Y,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LangUsageType
    {

        private string langCodeField;

        private string langUsageTextField;

        /// <remarks/>
        public string LangCode
        {
            get
            {
                return this.langCodeField;
            }
            set
            {
                this.langCodeField = value;
            }
        }

        /// <remarks/>
        public string LangUsageText
        {
            get
            {
                return this.langUsageTextField;
            }
            set
            {
                this.langUsageTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum MeasurementSystemCodeType
    {

        /// <remarks/>
        Imperial,

        /// <remarks/>
        Metric,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RemarkType
    {

        private bool displayIndField;

        private bool displayIndFieldSpecified;

        private string remarkTextField;

        private System.DateTime timestampField;

        private bool timestampFieldSpecified;

        /// <remarks/>
        public bool DisplayInd
        {
            get
            {
                return this.displayIndField;
            }
            set
            {
                this.displayIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DisplayIndSpecified
        {
            get
            {
                return this.displayIndFieldSpecified;
            }
            set
            {
                this.displayIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string RemarkText
        {
            get
            {
                return this.remarkTextField;
            }
            set
            {
                this.remarkTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime Timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimestampSpecified
        {
            get
            {
                return this.timestampFieldSpecified;
            }
            set
            {
                this.timestampFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SelectedOfferListType
    {

        private SelectedOfferType[] selectedOfferField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SelectedOffer")]
        public SelectedOfferType[] SelectedOffer
        {
            get
            {
                return this.selectedOfferField;
            }
            set
            {
                this.selectedOfferField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SelectedOfferType
    {

        private string offerRefIDField;

        private string ownerCodeField;

        private SelectedOfferItemType[] selectedOfferItemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OfferRefID
        {
            get
            {
                return this.offerRefIDField;
            }
            set
            {
                this.offerRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return this.ownerCodeField;
            }
            set
            {
                this.ownerCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SelectedOfferItem")]
        public SelectedOfferItemType[] SelectedOfferItem
        {
            get
            {
                return this.selectedOfferItemField;
            }
            set
            {
                this.selectedOfferItemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SelectedOfferItemType
    {

        private string offerItemRefIDField;

        private string[] paxRefIDField;

        private SelectedALaCarteOfferItemType selectedALaCarteOfferItemField;

        private SelectedBundleServicesType selectedBundleServicesField;

        private SelectedSeatType selectedSeatField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OfferItemRefID
        {
            get
            {
                return this.offerItemRefIDField;
            }
            set
            {
                this.offerItemRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        public SelectedALaCarteOfferItemType SelectedALaCarteOfferItem
        {
            get
            {
                return this.selectedALaCarteOfferItemField;
            }
            set
            {
                this.selectedALaCarteOfferItemField = value;
            }
        }

        /// <remarks/>
        public SelectedBundleServicesType SelectedBundleServices
        {
            get
            {
                return this.selectedBundleServicesField;
            }
            set
            {
                this.selectedBundleServicesField = value;
            }
        }

        /// <remarks/>
        public SelectedSeatType SelectedSeat
        {
            get
            {
                return this.selectedSeatField;
            }
            set
            {
                this.selectedSeatField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SelectedALaCarteOfferItemType
    {

        private OfferFlightAssociationsType offerFlightAssociationsField;

        private decimal qtyField;

        /// <remarks/>
        public OfferFlightAssociationsType OfferFlightAssociations
        {
            get
            {
                return this.offerFlightAssociationsField;
            }
            set
            {
                this.offerFlightAssociationsField = value;
            }
        }

        /// <remarks/>
        public decimal Qty
        {
            get
            {
                return this.qtyField;
            }
            set
            {
                this.qtyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferFlightAssociationsType
    {

        private object itemField;

        private ItemChoiceType1 itemElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DatedOperatingLegRef", typeof(DatedOperatingLegRefType))]
        [System.Xml.Serialization.XmlElementAttribute("PaxJourneyRef", typeof(PaxJourneyRefType))]
        [System.Xml.Serialization.XmlElementAttribute("PaxSegmentReferences", typeof(PaxSegmentRefListType))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemElementName")]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemChoiceType1 ItemElementName
        {
            get
            {
                return this.itemElementNameField;
            }
            set
            {
                this.itemElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedOperatingLegRefType
    {

        private string[] datedOperatingLegRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return this.datedOperatingLegRefIDField;
            }
            set
            {
                this.datedOperatingLegRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxJourneyRefType
    {

        private string[] paxJourneyRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxJourneyRefID", DataType = "token")]
        public string[] PaxJourneyRefID
        {
            get
            {
                return this.paxJourneyRefIDField;
            }
            set
            {
                this.paxJourneyRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxSegmentRefListType
    {

        private string[] paxSegmentRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return this.paxSegmentRefIDField;
            }
            set
            {
                this.paxSegmentRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes", IncludeInSchema = false)]
    public enum ItemChoiceType1
    {

        /// <remarks/>
        DatedOperatingLegRef,

        /// <remarks/>
        PaxJourneyRef,

        /// <remarks/>
        PaxSegmentReferences,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SelectedBundleServicesType
    {

        private string[] selectedServiceDefinitionRefIDField;

        private string selectedServiceRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SelectedServiceDefinitionRefID", DataType = "token")]
        public string[] SelectedServiceDefinitionRefID
        {
            get
            {
                return this.selectedServiceDefinitionRefIDField;
            }
            set
            {
                this.selectedServiceDefinitionRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SelectedServiceRefID
        {
            get
            {
                return this.selectedServiceRefIDField;
            }
            set
            {
                this.selectedServiceRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SelectedSeatType
    {

        private string columnIDField;

        private decimal seatRowNumberField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ColumnID
        {
            get
            {
                return this.columnIDField;
            }
            set
            {
                this.columnIDField = value;
            }
        }

        /// <remarks/>
        public decimal SeatRowNumber
        {
            get
            {
                return this.seatRowNumberField;
            }
            set
            {
                this.seatRowNumberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AcceptOrderItemListType
    {

        private CreateOrderItemType[] createOrderItemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CreateOrderItem")]
        public CreateOrderItemType[] CreateOrderItem
        {
            get
            {
                return this.createOrderItemField;
            }
            set
            {
                this.createOrderItemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CreateOrderItemType
    {

        private string offerItemRefIDField;

        private OfferItemTimeLimitsType[] offerItemTimeLimitsField;

        private OfferItemTypeType offerItemTypeField;

        private string ownerCodeField;

        private string webAddressURIField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OfferItemRefID
        {
            get
            {
                return this.offerItemRefIDField;
            }
            set
            {
                this.offerItemRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OfferItemTimeLimits")]
        public OfferItemTimeLimitsType[] OfferItemTimeLimits
        {
            get
            {
                return this.offerItemTimeLimitsField;
            }
            set
            {
                this.offerItemTimeLimitsField = value;
            }
        }

        /// <remarks/>
        public OfferItemTypeType OfferItemType
        {
            get
            {
                return this.offerItemTypeField;
            }
            set
            {
                this.offerItemTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return this.ownerCodeField;
            }
            set
            {
                this.ownerCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string WebAddressURI
        {
            get
            {
                return this.webAddressURIField;
            }
            set
            {
                this.webAddressURIField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferItemTimeLimitsType
    {

        private System.DateTime priceGuaranteeTimeLimitDateTimeField;

        private bool priceGuaranteeTimeLimitDateTimeFieldSpecified;

        /// <remarks/>
        public System.DateTime PriceGuaranteeTimeLimitDateTime
        {
            get
            {
                return this.priceGuaranteeTimeLimitDateTimeField;
            }
            set
            {
                this.priceGuaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PriceGuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return this.priceGuaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                this.priceGuaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferItemTypeType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BaggageItem", typeof(BaggageItemType))]
        [System.Xml.Serialization.XmlElementAttribute("FlightItem", typeof(FlightItemType))]
        [System.Xml.Serialization.XmlElementAttribute("OtherItem", typeof(OtherItemType))]
        [System.Xml.Serialization.XmlElementAttribute("SeatItem", typeof(SeatItemType))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BaggageItemType
    {

        private BagItemDetailsType[] bagItemDetailsField;

        private string paxRefIDField;

        private string paxSegmentRefIDField;

        private PriceType priceField;

        private string validatingCarrierCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BagItemDetails")]
        public BagItemDetailsType[] BagItemDetails
        {
            get
            {
                return this.bagItemDetailsField;
            }
            set
            {
                this.bagItemDetailsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxSegmentRefID
        {
            get
            {
                return this.paxSegmentRefIDField;
            }
            set
            {
                this.paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ValidatingCarrierCode
        {
            get
            {
                return this.validatingCarrierCodeField;
            }
            set
            {
                this.validatingCarrierCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BagItemDetailsType
    {

        private string baggageAllowanceRefIDField;

        private string baggageDisclosureRefIDField;

        private PriceType priceField;

        private string validatingCarrierCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string BaggageAllowanceRefID
        {
            get
            {
                return this.baggageAllowanceRefIDField;
            }
            set
            {
                this.baggageAllowanceRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string BaggageDisclosureRefID
        {
            get
            {
                return this.baggageDisclosureRefIDField;
            }
            set
            {
                this.baggageDisclosureRefIDField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ValidatingCarrierCode
        {
            get
            {
                return this.validatingCarrierCodeField;
            }
            set
            {
                this.validatingCarrierCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PriceType
    {

        private AmountType baseAmountField;

        private System.DateTime baseAmountGuaranteeTimeLimitDateTimeField;

        private bool baseAmountGuaranteeTimeLimitDateTimeFieldSpecified;

        private CurConversionType[] curConversionField;

        private DiscountType discountField;

        private AmountType equivAmountField;

        private FeeType[] feeField;

        private AmountType loyaltyUnitAmountField;

        private string loyaltyUnitNameField;

        private MarkupType[] markupField;

        private bool maskedIndField;

        private bool maskedIndFieldSpecified;

        private SurchargeType[] surchargeField;

        private TaxSummaryType[] taxSummaryField;

        private AmountType totalAmountField;

        /// <remarks/>
        public AmountType BaseAmount
        {
            get
            {
                return this.baseAmountField;
            }
            set
            {
                this.baseAmountField = value;
            }
        }

        /// <remarks/>
        public System.DateTime BaseAmountGuaranteeTimeLimitDateTime
        {
            get
            {
                return this.baseAmountGuaranteeTimeLimitDateTimeField;
            }
            set
            {
                this.baseAmountGuaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BaseAmountGuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return this.baseAmountGuaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                this.baseAmountGuaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CurConversion")]
        public CurConversionType[] CurConversion
        {
            get
            {
                return this.curConversionField;
            }
            set
            {
                this.curConversionField = value;
            }
        }

        /// <remarks/>
        public DiscountType Discount
        {
            get
            {
                return this.discountField;
            }
            set
            {
                this.discountField = value;
            }
        }

        /// <remarks/>
        public AmountType EquivAmount
        {
            get
            {
                return this.equivAmountField;
            }
            set
            {
                this.equivAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Fee")]
        public FeeType[] Fee
        {
            get
            {
                return this.feeField;
            }
            set
            {
                this.feeField = value;
            }
        }

        /// <remarks/>
        public AmountType LoyaltyUnitAmount
        {
            get
            {
                return this.loyaltyUnitAmountField;
            }
            set
            {
                this.loyaltyUnitAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string LoyaltyUnitName
        {
            get
            {
                return this.loyaltyUnitNameField;
            }
            set
            {
                this.loyaltyUnitNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Markup")]
        public MarkupType[] Markup
        {
            get
            {
                return this.markupField;
            }
            set
            {
                this.markupField = value;
            }
        }

        /// <remarks/>
        public bool MaskedInd
        {
            get
            {
                return this.maskedIndField;
            }
            set
            {
                this.maskedIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaskedIndSpecified
        {
            get
            {
                return this.maskedIndFieldSpecified;
            }
            set
            {
                this.maskedIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Surcharge")]
        public SurchargeType[] Surcharge
        {
            get
            {
                return this.surchargeField;
            }
            set
            {
                this.surchargeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TaxSummary")]
        public TaxSummaryType[] TaxSummary
        {
            get
            {
                return this.taxSummaryField;
            }
            set
            {
                this.taxSummaryField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalAmount
        {
            get
            {
                return this.totalAmountField;
            }
            set
            {
                this.totalAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DiscountType
    {

        private string appTextField;

        private string descTextField;

        private AmountType discountAmountField;

        private KeyValuePropertyType[] discountContextField;

        private decimal discountPercentField;

        private bool discountPercentFieldSpecified;

        private AmountType preDiscountedAmountField;

        /// <remarks/>
        public string AppText
        {
            get
            {
                return this.appTextField;
            }
            set
            {
                this.appTextField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType DiscountAmount
        {
            get
            {
                return this.discountAmountField;
            }
            set
            {
                this.discountAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DiscountContext")]
        public KeyValuePropertyType[] DiscountContext
        {
            get
            {
                return this.discountContextField;
            }
            set
            {
                this.discountContextField = value;
            }
        }

        /// <remarks/>
        public decimal DiscountPercent
        {
            get
            {
                return this.discountPercentField;
            }
            set
            {
                this.discountPercentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DiscountPercentSpecified
        {
            get
            {
                return this.discountPercentFieldSpecified;
            }
            set
            {
                this.discountPercentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType PreDiscountedAmount
        {
            get
            {
                return this.preDiscountedAmountField;
            }
            set
            {
                this.preDiscountedAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class KeyValuePropertyType
    {

        private string keyTextField;

        private PrefLevelType prefLevelField;

        private string valueTextField;

        /// <remarks/>
        public string KeyText
        {
            get
            {
                return this.keyTextField;
            }
            set
            {
                this.keyTextField = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return this.prefLevelField;
            }
            set
            {
                this.prefLevelField = value;
            }
        }

        /// <remarks/>
        public string ValueText
        {
            get
            {
                return this.valueTextField;
            }
            set
            {
                this.valueTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PrefLevelType
    {

        private PrefLevelCodeType prefLevelCodeField;

        /// <remarks/>
        public PrefLevelCodeType PrefLevelCode
        {
            get
            {
                return this.prefLevelCodeField;
            }
            set
            {
                this.prefLevelCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum PrefLevelCodeType
    {

        /// <remarks/>
        Exclude,

        /// <remarks/>
        Preferred,

        /// <remarks/>
        Required,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FeeType
    {

        private AmountType amountField;

        private bool approximateIndField;

        private bool approximateIndFieldSpecified;

        private string descTextField;

        private string desigTextField;

        private System.DateTime guaranteeTimeLimitDateTimeField;

        private bool guaranteeTimeLimitDateTimeFieldSpecified;

        private AmountType localAmountField;

        private AmountType maximumAmountField;

        private AmountType minimumAmountField;

        private decimal percentField;

        private bool percentFieldSpecified;

        private bool refundIndField;

        private bool refundIndFieldSpecified;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return this.approximateIndField;
            }
            set
            {
                this.approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return this.approximateIndFieldSpecified;
            }
            set
            {
                this.approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public string DesigText
        {
            get
            {
                return this.desigTextField;
            }
            set
            {
                this.desigTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime GuaranteeTimeLimitDateTime
        {
            get
            {
                return this.guaranteeTimeLimitDateTimeField;
            }
            set
            {
                this.guaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return this.guaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                this.guaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType LocalAmount
        {
            get
            {
                return this.localAmountField;
            }
            set
            {
                this.localAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MaximumAmount
        {
            get
            {
                return this.maximumAmountField;
            }
            set
            {
                this.maximumAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MinimumAmount
        {
            get
            {
                return this.minimumAmountField;
            }
            set
            {
                this.minimumAmountField = value;
            }
        }

        /// <remarks/>
        public decimal Percent
        {
            get
            {
                return this.percentField;
            }
            set
            {
                this.percentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PercentSpecified
        {
            get
            {
                return this.percentFieldSpecified;
            }
            set
            {
                this.percentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool RefundInd
        {
            get
            {
                return this.refundIndField;
            }
            set
            {
                this.refundIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RefundIndSpecified
        {
            get
            {
                return this.refundIndFieldSpecified;
            }
            set
            {
                this.refundIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MarkupType
    {

        private AmountType amountField;

        private string typeCodeField;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return this.typeCodeField;
            }
            set
            {
                this.typeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SurchargeType
    {

        private bool allRefundableIndField;

        private bool allRefundableIndFieldSpecified;

        private FeeType[] breakdownField;

        private AmountType totalAmountField;

        /// <remarks/>
        public bool AllRefundableInd
        {
            get
            {
                return this.allRefundableIndField;
            }
            set
            {
                this.allRefundableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllRefundableIndSpecified
        {
            get
            {
                return this.allRefundableIndFieldSpecified;
            }
            set
            {
                this.allRefundableIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Breakdown")]
        public FeeType[] Breakdown
        {
            get
            {
                return this.breakdownField;
            }
            set
            {
                this.breakdownField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalAmount
        {
            get
            {
                return this.totalAmountField;
            }
            set
            {
                this.totalAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FlightItemType
    {

        private FareDetailType fareDetailField;

        private string[] originDestRefIDField;

        private PriceType priceField;

        /// <remarks/>
        public FareDetailType FareDetail
        {
            get
            {
                return this.fareDetailField;
            }
            set
            {
                this.fareDetailField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OriginDestRefID", DataType = "token")]
        public string[] OriginDestRefID
        {
            get
            {
                return this.originDestRefIDField;
            }
            set
            {
                this.originDestRefIDField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareDetailType
    {

        private string accountCodeField;

        private FareCalculationInfoType fareCalculationInfoField;

        private FareComponentType[] fareComponentField;

        private string fareIndCodeField;

        private FarePriceTypeType[] farePriceTypeField;

        private string fareRefTextField;

        private FareWaiverType[] fareWaiverField;

        private bool filedFareIndField;

        private bool filedFareIndFieldSpecified;

        private string netReportingCodeTextField;

        private string[] paxRefIDField;

        private string[] penaltyRefIDField;

        private PriceType priceField;

        private string pricingSystemCodeTextField;

        private string statisticalCodeTextField;

        private string tourCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AccountCode
        {
            get
            {
                return this.accountCodeField;
            }
            set
            {
                this.accountCodeField = value;
            }
        }

        /// <remarks/>
        public FareCalculationInfoType FareCalculationInfo
        {
            get
            {
                return this.fareCalculationInfoField;
            }
            set
            {
                this.fareCalculationInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareComponent")]
        public FareComponentType[] FareComponent
        {
            get
            {
                return this.fareComponentField;
            }
            set
            {
                this.fareComponentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FareIndCode
        {
            get
            {
                return this.fareIndCodeField;
            }
            set
            {
                this.fareIndCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FarePriceType")]
        public FarePriceTypeType[] FarePriceType
        {
            get
            {
                return this.farePriceTypeField;
            }
            set
            {
                this.farePriceTypeField = value;
            }
        }

        /// <remarks/>
        public string FareRefText
        {
            get
            {
                return this.fareRefTextField;
            }
            set
            {
                this.fareRefTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareWaiver")]
        public FareWaiverType[] FareWaiver
        {
            get
            {
                return this.fareWaiverField;
            }
            set
            {
                this.fareWaiverField = value;
            }
        }

        /// <remarks/>
        public bool FiledFareInd
        {
            get
            {
                return this.filedFareIndField;
            }
            set
            {
                this.filedFareIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FiledFareIndSpecified
        {
            get
            {
                return this.filedFareIndFieldSpecified;
            }
            set
            {
                this.filedFareIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string NetReportingCodeText
        {
            get
            {
                return this.netReportingCodeTextField;
            }
            set
            {
                this.netReportingCodeTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PenaltyRefID", DataType = "token")]
        public string[] PenaltyRefID
        {
            get
            {
                return this.penaltyRefIDField;
            }
            set
            {
                this.penaltyRefIDField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
        public string PricingSystemCodeText
        {
            get
            {
                return this.pricingSystemCodeTextField;
            }
            set
            {
                this.pricingSystemCodeTextField = value;
            }
        }

        /// <remarks/>
        public string StatisticalCodeText
        {
            get
            {
                return this.statisticalCodeTextField;
            }
            set
            {
                this.statisticalCodeTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TourCode
        {
            get
            {
                return this.tourCodeField;
            }
            set
            {
                this.tourCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareCalculationInfoType
    {

        private string addlInfoTextField;

        private string pricingCodeTextField;

        private string reportingCodeTextField;

        /// <remarks/>
        public string AddlInfoText
        {
            get
            {
                return this.addlInfoTextField;
            }
            set
            {
                this.addlInfoTextField = value;
            }
        }

        /// <remarks/>
        public string PricingCodeText
        {
            get
            {
                return this.pricingCodeTextField;
            }
            set
            {
                this.pricingCodeTextField = value;
            }
        }

        /// <remarks/>
        public string ReportingCodeText
        {
            get
            {
                return this.reportingCodeTextField;
            }
            set
            {
                this.reportingCodeTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareComponentType
    {

        private CabinTypeType cabinTypeField;

        private CancelRestrictionsType[] cancelRestrictionsField;

        private ChangeRestrictionsType[] changeRestrictionsField;

        private string fareBasisAppCodeField;

        private string fareBasisCityPairTextField;

        private string fareBasisCodeField;

        private FareRuleType[] fareRuleField;

        private string fareTypeCodeField;

        private string negotiatedCodeField;

        private string[] paxSegmentRefIDField;

        private PriceType priceField;

        private string priceClassRefIDField;

        private RBD_Type rBDField;

        private string ticketDesigCodeField;

        /// <remarks/>
        public CabinTypeType CabinType
        {
            get
            {
                return this.cabinTypeField;
            }
            set
            {
                this.cabinTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CancelRestrictions")]
        public CancelRestrictionsType[] CancelRestrictions
        {
            get
            {
                return this.cancelRestrictionsField;
            }
            set
            {
                this.cancelRestrictionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ChangeRestrictions")]
        public ChangeRestrictionsType[] ChangeRestrictions
        {
            get
            {
                return this.changeRestrictionsField;
            }
            set
            {
                this.changeRestrictionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FareBasisAppCode
        {
            get
            {
                return this.fareBasisAppCodeField;
            }
            set
            {
                this.fareBasisAppCodeField = value;
            }
        }

        /// <remarks/>
        public string FareBasisCityPairText
        {
            get
            {
                return this.fareBasisCityPairTextField;
            }
            set
            {
                this.fareBasisCityPairTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FareBasisCode
        {
            get
            {
                return this.fareBasisCodeField;
            }
            set
            {
                this.fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareRule")]
        public FareRuleType[] FareRule
        {
            get
            {
                return this.fareRuleField;
            }
            set
            {
                this.fareRuleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FareTypeCode
        {
            get
            {
                return this.fareTypeCodeField;
            }
            set
            {
                this.fareTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string NegotiatedCode
        {
            get
            {
                return this.negotiatedCodeField;
            }
            set
            {
                this.negotiatedCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return this.paxSegmentRefIDField;
            }
            set
            {
                this.paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PriceClassRefID
        {
            get
            {
                return this.priceClassRefIDField;
            }
            set
            {
                this.priceClassRefIDField = value;
            }
        }

        /// <remarks/>
        public RBD_Type RBD
        {
            get
            {
                return this.rBDField;
            }
            set
            {
                this.rBDField = value;
            }
        }

        /// <remarks/>
        public string TicketDesigCode
        {
            get
            {
                return this.ticketDesigCodeField;
            }
            set
            {
                this.ticketDesigCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CabinTypeType
    {

        private string cabinTypeCodeField;

        private string cabinTypeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CabinTypeCode
        {
            get
            {
                return this.cabinTypeCodeField;
            }
            set
            {
                this.cabinTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CabinTypeName
        {
            get
            {
                return this.cabinTypeNameField;
            }
            set
            {
                this.cabinTypeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CancelRestrictionsType
    {

        private bool allowedModificationIndField;

        private bool allowedModificationIndFieldSpecified;

        private string descTextField;

        private System.DateTime effectiveDateTimeField;

        private bool effectiveDateTimeFieldSpecified;

        private System.DateTime expirationDateTimeField;

        private bool expirationDateTimeFieldSpecified;

        private ChangeOrCancelFeeType feeField;

        private JourneyStageTypeCodeType journeyStageCodeField;

        private bool journeyStageCodeFieldSpecified;

        private string paxRefIDField;

        private RefundType refundField;

        /// <remarks/>
        public bool AllowedModificationInd
        {
            get
            {
                return this.allowedModificationIndField;
            }
            set
            {
                this.allowedModificationIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllowedModificationIndSpecified
        {
            get
            {
                return this.allowedModificationIndFieldSpecified;
            }
            set
            {
                this.allowedModificationIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime EffectiveDateTime
        {
            get
            {
                return this.effectiveDateTimeField;
            }
            set
            {
                this.effectiveDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EffectiveDateTimeSpecified
        {
            get
            {
                return this.effectiveDateTimeFieldSpecified;
            }
            set
            {
                this.effectiveDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime ExpirationDateTime
        {
            get
            {
                return this.expirationDateTimeField;
            }
            set
            {
                this.expirationDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpirationDateTimeSpecified
        {
            get
            {
                return this.expirationDateTimeFieldSpecified;
            }
            set
            {
                this.expirationDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public ChangeOrCancelFeeType Fee
        {
            get
            {
                return this.feeField;
            }
            set
            {
                this.feeField = value;
            }
        }

        /// <remarks/>
        public JourneyStageTypeCodeType JourneyStageCode
        {
            get
            {
                return this.journeyStageCodeField;
            }
            set
            {
                this.journeyStageCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool JourneyStageCodeSpecified
        {
            get
            {
                return this.journeyStageCodeFieldSpecified;
            }
            set
            {
                this.journeyStageCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        public RefundType Refund
        {
            get
            {
                return this.refundField;
            }
            set
            {
                this.refundField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ChangeOrCancelFeeType
    {

        private AmountType amountField;

        private bool approximateIndField;

        private bool approximateIndFieldSpecified;

        private string descTextField;

        private AmountType maximumAmountField;

        private AmountType minimumAmountField;

        private decimal percentField;

        private bool percentFieldSpecified;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return this.approximateIndField;
            }
            set
            {
                this.approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return this.approximateIndFieldSpecified;
            }
            set
            {
                this.approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType MaximumAmount
        {
            get
            {
                return this.maximumAmountField;
            }
            set
            {
                this.maximumAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MinimumAmount
        {
            get
            {
                return this.minimumAmountField;
            }
            set
            {
                this.minimumAmountField = value;
            }
        }

        /// <remarks/>
        public decimal Percent
        {
            get
            {
                return this.percentField;
            }
            set
            {
                this.percentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PercentSpecified
        {
            get
            {
                return this.percentFieldSpecified;
            }
            set
            {
                this.percentFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum JourneyStageTypeCodeType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("After Departure")]
        AfterDeparture,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("No Show")]
        NoShow,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Prior To Departure")]
        PriorToDeparture,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RefundType
    {

        private string descTextField;

        private AmountType fixedAmountField;

        private RefundLevelTypeCodeType levelTypeCodeField;

        private bool levelTypeCodeFieldSpecified;

        private AmountType maximumAmountField;

        private AmountType minimumAmountField;

        private string paymentTypeCodeField;

        private decimal percentField;

        private bool percentFieldSpecified;

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType FixedAmount
        {
            get
            {
                return this.fixedAmountField;
            }
            set
            {
                this.fixedAmountField = value;
            }
        }

        /// <remarks/>
        public RefundLevelTypeCodeType LevelTypeCode
        {
            get
            {
                return this.levelTypeCodeField;
            }
            set
            {
                this.levelTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LevelTypeCodeSpecified
        {
            get
            {
                return this.levelTypeCodeFieldSpecified;
            }
            set
            {
                this.levelTypeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType MaximumAmount
        {
            get
            {
                return this.maximumAmountField;
            }
            set
            {
                this.maximumAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MinimumAmount
        {
            get
            {
                return this.minimumAmountField;
            }
            set
            {
                this.minimumAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaymentTypeCode
        {
            get
            {
                return this.paymentTypeCodeField;
            }
            set
            {
                this.paymentTypeCodeField = value;
            }
        }

        /// <remarks/>
        public decimal Percent
        {
            get
            {
                return this.percentField;
            }
            set
            {
                this.percentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PercentSpecified
        {
            get
            {
                return this.percentFieldSpecified;
            }
            set
            {
                this.percentFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum RefundLevelTypeCodeType
    {

        /// <remarks/>
        Full,

        /// <remarks/>
        None,

        /// <remarks/>
        Partial,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ChangeRestrictionsType
    {

        private bool allowedModificationIndField;

        private bool allowedModificationIndFieldSpecified;

        private ChangeTypeCodeType changeTypeCodeField;

        private bool changeTypeCodeFieldSpecified;

        private string descTextField;

        private System.DateTime effectiveDateTimeField;

        private bool effectiveDateTimeFieldSpecified;

        private System.DateTime expirationDateTimeField;

        private bool expirationDateTimeFieldSpecified;

        private ChangeOrCancelFeeType feeField;

        private JourneyStageTypeCodeType journeyStageCodeField;

        private bool journeyStageCodeFieldSpecified;

        private string paxRefIDField;

        private RefundType refundField;

        /// <remarks/>
        public bool AllowedModificationInd
        {
            get
            {
                return this.allowedModificationIndField;
            }
            set
            {
                this.allowedModificationIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AllowedModificationIndSpecified
        {
            get
            {
                return this.allowedModificationIndFieldSpecified;
            }
            set
            {
                this.allowedModificationIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public ChangeTypeCodeType ChangeTypeCode
        {
            get
            {
                return this.changeTypeCodeField;
            }
            set
            {
                this.changeTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ChangeTypeCodeSpecified
        {
            get
            {
                return this.changeTypeCodeFieldSpecified;
            }
            set
            {
                this.changeTypeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime EffectiveDateTime
        {
            get
            {
                return this.effectiveDateTimeField;
            }
            set
            {
                this.effectiveDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EffectiveDateTimeSpecified
        {
            get
            {
                return this.effectiveDateTimeFieldSpecified;
            }
            set
            {
                this.effectiveDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime ExpirationDateTime
        {
            get
            {
                return this.expirationDateTimeField;
            }
            set
            {
                this.expirationDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpirationDateTimeSpecified
        {
            get
            {
                return this.expirationDateTimeFieldSpecified;
            }
            set
            {
                this.expirationDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public ChangeOrCancelFeeType Fee
        {
            get
            {
                return this.feeField;
            }
            set
            {
                this.feeField = value;
            }
        }

        /// <remarks/>
        public JourneyStageTypeCodeType JourneyStageCode
        {
            get
            {
                return this.journeyStageCodeField;
            }
            set
            {
                this.journeyStageCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool JourneyStageCodeSpecified
        {
            get
            {
                return this.journeyStageCodeFieldSpecified;
            }
            set
            {
                this.journeyStageCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        public RefundType Refund
        {
            get
            {
                return this.refundField;
            }
            set
            {
                this.refundField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum ChangeTypeCodeType
    {

        /// <remarks/>
        Flight,

        /// <remarks/>
        Name,

        /// <remarks/>
        Service,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareRuleType
    {

        private string instantPurchaseTypeCodeField;

        private RemarkType[] remarkField;

        private string ruleCodeField;

        private bool ticketlessIndField;

        private bool ticketlessIndFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string InstantPurchaseTypeCode
        {
            get
            {
                return this.instantPurchaseTypeCodeField;
            }
            set
            {
                this.instantPurchaseTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return this.remarkField;
            }
            set
            {
                this.remarkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string RuleCode
        {
            get
            {
                return this.ruleCodeField;
            }
            set
            {
                this.ruleCodeField = value;
            }
        }

        /// <remarks/>
        public bool TicketlessInd
        {
            get
            {
                return this.ticketlessIndField;
            }
            set
            {
                this.ticketlessIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TicketlessIndSpecified
        {
            get
            {
                return this.ticketlessIndFieldSpecified;
            }
            set
            {
                this.ticketlessIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RBD_Type
    {

        private string rBD_CodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string RBD_Code
        {
            get
            {
                return this.rBD_CodeField;
            }
            set
            {
                this.rBD_CodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FarePriceTypeType
    {

        private FarePriceTypeCodeType farePriceTypeCodeField;

        private PriceType priceField;

        /// <remarks/>
        public FarePriceTypeCodeType FarePriceTypeCode
        {
            get
            {
                return this.farePriceTypeCodeField;
            }
            set
            {
                this.farePriceTypeCodeField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum FarePriceTypeCodeType
    {

        /// <remarks/>
        Filed,

        /// <remarks/>
        Net,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareWaiverType
    {

        private string fareRuleWaiverCodeField;

        private string fareWaiverTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FareRuleWaiverCode
        {
            get
            {
                return this.fareRuleWaiverCodeField;
            }
            set
            {
                this.fareRuleWaiverCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FareWaiverTypeCode
        {
            get
            {
                return this.fareWaiverTypeCodeField;
            }
            set
            {
                this.fareWaiverTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OtherItemType
    {

        private string descTextField;

        private PriceType priceField;

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SeatItemType
    {

        private string columnIDField;

        private string[] datedOperatingLegRefIDField;

        private string paxRefIDField;

        private string paxSegmentRefIDField;

        private PriceType priceField;

        private string rowNumberField;

        private string[] seatProfileRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ColumnID
        {
            get
            {
                return this.columnIDField;
            }
            set
            {
                this.columnIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return this.datedOperatingLegRefIDField;
            }
            set
            {
                this.datedOperatingLegRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxSegmentRefID
        {
            get
            {
                return this.paxSegmentRefIDField;
            }
            set
            {
                this.paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        public PriceType Price
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
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RowNumber
        {
            get
            {
                return this.rowNumberField;
            }
            set
            {
                this.rowNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SeatProfileRefID", DataType = "token")]
        public string[] SeatProfileRefID
        {
            get
            {
                return this.seatProfileRefIDField;
            }
            set
            {
                this.seatProfileRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PricedOfferType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AcceptOrderItemList", typeof(AcceptOrderItemListType))]
        [System.Xml.Serialization.XmlElementAttribute("SelectedOfferList", typeof(SelectedOfferListType))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PolicyNodeInfoType
    {

        private string pathTextField;

        private string tagNameField;

        /// <remarks/>
        public string PathText
        {
            get
            {
                return this.pathTextField;
            }
            set
            {
                this.pathTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TagName
        {
            get
            {
                return this.tagNameField;
            }
            set
            {
                this.tagNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PolicyType
    {

        private DescType[] descField;

        private string ownerNameField;

        private PolicyNodeInfoType[] policyNodeInfoField;

        private string policyTypeTextField;

        private decimal versionNumberField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Desc")]
        public DescType[] Desc
        {
            get
            {
                return this.descField;
            }
            set
            {
                this.descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return this.ownerNameField;
            }
            set
            {
                this.ownerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PolicyNodeInfo")]
        public PolicyNodeInfoType[] PolicyNodeInfo
        {
            get
            {
                return this.policyNodeInfoField;
            }
            set
            {
                this.policyNodeInfoField = value;
            }
        }

        /// <remarks/>
        public string PolicyTypeText
        {
            get
            {
                return this.policyTypeTextField;
            }
            set
            {
                this.policyTypeTextField = value;
            }
        }

        /// <remarks/>
        public decimal VersionNumber
        {
            get
            {
                return this.versionNumberField;
            }
            set
            {
                this.versionNumberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DescType
    {

        private string descTextField;

        private string markupStyleTextField;

        private string uRLField;

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public string MarkupStyleText
        {
            get
            {
                return this.markupStyleTextField;
            }
            set
            {
                this.markupStyleTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return this.uRLField;
            }
            set
            {
                this.uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SecurePaymentAuthenticationVersionType
    {

        private string[] cardEnrollmentVersionTextField;

        private string[] supportedVersionTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CardEnrollmentVersionText")]
        public string[] CardEnrollmentVersionText
        {
            get
            {
                return this.cardEnrollmentVersionTextField;
            }
            set
            {
                this.cardEnrollmentVersionTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SupportedVersionText")]
        public string[] SupportedVersionText
        {
            get
            {
                return this.supportedVersionTextField;
            }
            set
            {
                this.supportedVersionTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentCardCriteriaAddlInfoType
    {

        private string cardProductTypeCodeField;

        private string cardTypeTextField;

        private SecurePaymentAuthenticationVersionType securePaymentAuthenticationVersionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CardProductTypeCode
        {
            get
            {
                return this.cardProductTypeCodeField;
            }
            set
            {
                this.cardProductTypeCodeField = value;
            }
        }

        /// <remarks/>
        public string CardTypeText
        {
            get
            {
                return this.cardTypeTextField;
            }
            set
            {
                this.cardTypeTextField = value;
            }
        }

        /// <remarks/>
        public SecurePaymentAuthenticationVersionType SecurePaymentAuthenticationVersion
        {
            get
            {
                return this.securePaymentAuthenticationVersionField;
            }
            set
            {
                this.securePaymentAuthenticationVersionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentCriteriaAddlInfoType
    {

        private PaymentCardCriteriaAddlInfoType paymentCardCriteriaAddlInfoField;

        private RemarkType[] paymentOtherMethodAddlInfoField;

        /// <remarks/>
        public PaymentCardCriteriaAddlInfoType PaymentCardCriteriaAddlInfo
        {
            get
            {
                return this.paymentCardCriteriaAddlInfoField;
            }
            set
            {
                this.paymentCardCriteriaAddlInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Remark", IsNullable = false)]
        public RemarkType[] PaymentOtherMethodAddlInfo
        {
            get
            {
                return this.paymentOtherMethodAddlInfoField;
            }
            set
            {
                this.paymentOtherMethodAddlInfoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ClearanceCriteriaType
    {

        private string processRuleCodeField;

        private System.DateTime remittanceDateField;

        private bool remittanceDateFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ProcessRuleCode
        {
            get
            {
                return this.processRuleCodeField;
            }
            set
            {
                this.processRuleCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime RemittanceDate
        {
            get
            {
                return this.remittanceDateField;
            }
            set
            {
                this.remittanceDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RemittanceDateSpecified
        {
            get
            {
                return this.remittanceDateFieldSpecified;
            }
            set
            {
                this.remittanceDateFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentMethodCriteriaType
    {

        private ClearanceCriteriaType[] clearanceCriteriaField;

        private string paymentBrandCodeField;

        private PaymentCriteriaAddlInfoType paymentCriteriaAddlInfoField;

        private string paymentMethodCountryCodeField;

        private string paymentTypeCodeField;

        private PrefLevelType prefLevelField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ClearanceCriteria")]
        public ClearanceCriteriaType[] ClearanceCriteria
        {
            get
            {
                return this.clearanceCriteriaField;
            }
            set
            {
                this.clearanceCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaymentBrandCode
        {
            get
            {
                return this.paymentBrandCodeField;
            }
            set
            {
                this.paymentBrandCodeField = value;
            }
        }

        /// <remarks/>
        public PaymentCriteriaAddlInfoType PaymentCriteriaAddlInfo
        {
            get
            {
                return this.paymentCriteriaAddlInfoField;
            }
            set
            {
                this.paymentCriteriaAddlInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaymentMethodCountryCode
        {
            get
            {
                return this.paymentMethodCountryCodeField;
            }
            set
            {
                this.paymentMethodCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaymentTypeCode
        {
            get
            {
                return this.paymentTypeCodeField;
            }
            set
            {
                this.paymentTypeCodeField = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return this.prefLevelField;
            }
            set
            {
                this.prefLevelField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferAssociationType
    {

        private string[] offerItemRefIDField;

        private string offerRefIDField;

        private string ownerCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OfferItemRefID", DataType = "token")]
        public string[] OfferItemRefID
        {
            get
            {
                return this.offerItemRefIDField;
            }
            set
            {
                this.offerItemRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OfferRefID
        {
            get
            {
                return this.offerRefIDField;
            }
            set
            {
                this.offerRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return this.ownerCodeField;
            }
            set
            {
                this.ownerCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentAssociatedOfferFunctionsCriteriaType
    {

        private OfferAssociationType[] offerAssociationField;

        private PaymentMethodCriteriaType paymentMethodCriteriaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OfferAssociation")]
        public OfferAssociationType[] OfferAssociation
        {
            get
            {
                return this.offerAssociationField;
            }
            set
            {
                this.offerAssociationField = value;
            }
        }

        /// <remarks/>
        public PaymentMethodCriteriaType PaymentMethodCriteria
        {
            get
            {
                return this.paymentMethodCriteriaField;
            }
            set
            {
                this.paymentMethodCriteriaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceCriteriaType
    {

        private bool includeIndField;

        private bool includeIndFieldSpecified;

        private PrefLevelType prefLevelField;

        private string rFICField;

        private string[] rFISCField;

        private bool servicePricingOnlyIndField;

        private bool servicePricingOnlyIndFieldSpecified;

        private string[] taxonomyCodeField;

        /// <remarks/>
        public bool IncludeInd
        {
            get
            {
                return this.includeIndField;
            }
            set
            {
                this.includeIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IncludeIndSpecified
        {
            get
            {
                return this.includeIndFieldSpecified;
            }
            set
            {
                this.includeIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return this.prefLevelField;
            }
            set
            {
                this.prefLevelField = value;
            }
        }

        /// <remarks/>
        public string RFIC
        {
            get
            {
                return this.rFICField;
            }
            set
            {
                this.rFICField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RFISC")]
        public string[] RFISC
        {
            get
            {
                return this.rFISCField;
            }
            set
            {
                this.rFISCField = value;
            }
        }

        /// <remarks/>
        public bool ServicePricingOnlyInd
        {
            get
            {
                return this.servicePricingOnlyIndField;
            }
            set
            {
                this.servicePricingOnlyIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ServicePricingOnlyIndSpecified
        {
            get
            {
                return this.servicePricingOnlyIndFieldSpecified;
            }
            set
            {
                this.servicePricingOnlyIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TaxonomyCode", DataType = "token")]
        public string[] TaxonomyCode
        {
            get
            {
                return this.taxonomyCodeField;
            }
            set
            {
                this.taxonomyCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SeatCriteriaType
    {

        private string columnIDField;

        private string rowNumberField;

        private string[] seatCharacteristicCodeField;

        private string[] seatProfileCriteriaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ColumnID
        {
            get
            {
                return this.columnIDField;
            }
            set
            {
                this.columnIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string RowNumber
        {
            get
            {
                return this.rowNumberField;
            }
            set
            {
                this.rowNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SeatCharacteristicCode", DataType = "token")]
        public string[] SeatCharacteristicCode
        {
            get
            {
                return this.seatCharacteristicCodeField;
            }
            set
            {
                this.seatCharacteristicCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("SeatCharacteristicCode", typeof(string), DataType = "token", IsNullable = false)]
        public string[] SeatProfileCriteria
        {
            get
            {
                return this.seatProfileCriteriaField;
            }
            set
            {
                this.seatProfileCriteriaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TravelAgentType
    {

        private string travelAgentIDField;

        private TravelAgencyTypeCodeType typeCodeField;

        private bool typeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TravelAgentID
        {
            get
            {
                return this.travelAgentIDField;
            }
            set
            {
                this.travelAgentIDField = value;
            }
        }

        /// <remarks/>
        public TravelAgencyTypeCodeType TypeCode
        {
            get
            {
                return this.typeCodeField;
            }
            set
            {
                this.typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeCodeSpecified
        {
            get
            {
                return this.typeCodeFieldSpecified;
            }
            set
            {
                this.typeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TravelAgencyTypeCodeType
    {

        /// <remarks/>
        OnlineTravelAgency,

        /// <remarks/>
        TravelAgency,

        /// <remarks/>
        TravelManagementCompany,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TravelAgencyType
    {

        private string agencyIDField;

        private ContactInfoType[] contactInfoField;

        private decimal iATA_NumberField;

        private bool iATA_NumberFieldSpecified;

        private string nameField;

        private string pseudoCityIDField;

        private TravelAgentType travelAgentField;

        private TravelAgencyTypeCodeType typeCodeField;

        private bool typeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AgencyID
        {
            get
            {
                return this.agencyIDField;
            }
            set
            {
                this.agencyIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ContactInfo")]
        public ContactInfoType[] ContactInfo
        {
            get
            {
                return this.contactInfoField;
            }
            set
            {
                this.contactInfoField = value;
            }
        }

        /// <remarks/>
        public decimal IATA_Number
        {
            get
            {
                return this.iATA_NumberField;
            }
            set
            {
                this.iATA_NumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IATA_NumberSpecified
        {
            get
            {
                return this.iATA_NumberFieldSpecified;
            }
            set
            {
                this.iATA_NumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PseudoCityID
        {
            get
            {
                return this.pseudoCityIDField;
            }
            set
            {
                this.pseudoCityIDField = value;
            }
        }

        /// <remarks/>
        public TravelAgentType TravelAgent
        {
            get
            {
                return this.travelAgentField;
            }
            set
            {
                this.travelAgentField = value;
            }
        }

        /// <remarks/>
        public TravelAgencyTypeCodeType TypeCode
        {
            get
            {
                return this.typeCodeField;
            }
            set
            {
                this.typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeCodeSpecified
        {
            get
            {
                return this.typeCodeFieldSpecified;
            }
            set
            {
                this.typeCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PromotionIssuerType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Carrier", typeof(CarrierType))]
        [System.Xml.Serialization.XmlElementAttribute("Org", typeof(OrgType))]
        [System.Xml.Serialization.XmlElementAttribute("TravelAgency", typeof(TravelAgencyType))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CarrierType
    {

        private string airlineDesigCodeField;

        private bool duplicateDesigIndField;

        private bool duplicateDesigIndFieldSpecified;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AirlineDesigCode
        {
            get
            {
                return this.airlineDesigCodeField;
            }
            set
            {
                this.airlineDesigCodeField = value;
            }
        }

        /// <remarks/>
        public bool DuplicateDesigInd
        {
            get
            {
                return this.duplicateDesigIndField;
            }
            set
            {
                this.duplicateDesigIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DuplicateDesigIndSpecified
        {
            get
            {
                return this.duplicateDesigIndFieldSpecified;
            }
            set
            {
                this.duplicateDesigIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OrgType
    {

        private ContactInfoType[] contactInfoField;

        private string nameField;

        private string orgIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ContactInfo")]
        public ContactInfoType[] ContactInfo
        {
            get
            {
                return this.contactInfoField;
            }
            set
            {
                this.contactInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OrgID
        {
            get
            {
                return this.orgIDField;
            }
            set
            {
                this.orgIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PromotionType
    {

        private string ownerNameField;

        private string[] paxRefIDField;

        private string promotionIDField;

        private PromotionIssuerType promotionIssuerField;

        private RemarkType[] remarkField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return this.ownerNameField;
            }
            set
            {
                this.ownerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PromotionID
        {
            get
            {
                return this.promotionIDField;
            }
            set
            {
                this.promotionIDField = value;
            }
        }

        /// <remarks/>
        public PromotionIssuerType PromotionIssuer
        {
            get
            {
                return this.promotionIssuerField;
            }
            set
            {
                this.promotionIssuerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return this.remarkField;
            }
            set
            {
                this.remarkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return this.uRLField;
            }
            set
            {
                this.uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgramOwnerType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Carrier", typeof(CarrierType))]
        [System.Xml.Serialization.XmlElementAttribute("Org", typeof(OrgType))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgramAccountType
    {

        private string accountIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AccountID
        {
            get
            {
                return this.accountIDField;
            }
            set
            {
                this.accountIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgamContractType
    {

        private string contractIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ContractID
        {
            get
            {
                return this.contractIDField;
            }
            set
            {
                this.contractIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PrePaidCertificateType
    {

        private string appTextField;

        private System.DateTime effectiveDateTimeField;

        private bool effectiveDateTimeFieldSpecified;

        private System.DateTime expiryDateTimeField;

        private bool expiryDateTimeFieldSpecified;

        private decimal numberField;

        private bool numberFieldSpecified;

        /// <remarks/>
        public string AppText
        {
            get
            {
                return this.appTextField;
            }
            set
            {
                this.appTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime EffectiveDateTime
        {
            get
            {
                return this.effectiveDateTimeField;
            }
            set
            {
                this.effectiveDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EffectiveDateTimeSpecified
        {
            get
            {
                return this.effectiveDateTimeFieldSpecified;
            }
            set
            {
                this.effectiveDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime ExpiryDateTime
        {
            get
            {
                return this.expiryDateTimeField;
            }
            set
            {
                this.expiryDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpiryDateTimeSpecified
        {
            get
            {
                return this.expiryDateTimeFieldSpecified;
            }
            set
            {
                this.expiryDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal Number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NumberSpecified
        {
            get
            {
                return this.numberFieldSpecified;
            }
            set
            {
                this.numberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ProgramCriteriaType
    {

        private string companyIndexTextField;

        private string memberStatusTextField;

        private string nameField;

        private PrePaidCertificateType[] prePaidCertificateField;

        private ProgamContractType progamContractField;

        private ProgramAccountType[] programAccountField;

        private ProgramOwnerType programOwnerField;

        private string typeCodeField;

        /// <remarks/>
        public string CompanyIndexText
        {
            get
            {
                return this.companyIndexTextField;
            }
            set
            {
                this.companyIndexTextField = value;
            }
        }

        /// <remarks/>
        public string MemberStatusText
        {
            get
            {
                return this.memberStatusTextField;
            }
            set
            {
                this.memberStatusTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PrePaidCertificate")]
        public PrePaidCertificateType[] PrePaidCertificate
        {
            get
            {
                return this.prePaidCertificateField;
            }
            set
            {
                this.prePaidCertificateField = value;
            }
        }

        /// <remarks/>
        public ProgamContractType ProgamContract
        {
            get
            {
                return this.progamContractField;
            }
            set
            {
                this.progamContractField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ProgramAccount")]
        public ProgramAccountType[] ProgramAccount
        {
            get
            {
                return this.programAccountField;
            }
            set
            {
                this.programAccountField = value;
            }
        }

        /// <remarks/>
        public ProgramOwnerType ProgramOwner
        {
            get
            {
                return this.programOwnerField;
            }
            set
            {
                this.programOwnerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return this.typeCodeField;
            }
            set
            {
                this.typeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PricingMethodCriteriaType
    {

        private string bestPricingOptionTextField;

        /// <remarks/>
        public string BestPricingOptionText
        {
            get
            {
                return this.bestPricingOptionTextField;
            }
            set
            {
                this.bestPricingOptionTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class GroupFareCriteriaType
    {

        private string[] fareBasisCodeField;

        private string[] paxSegmentRefIDField;

        private PrefLevelType prefLevelField;

        private string pTCField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareBasisCode", DataType = "token")]
        public string[] FareBasisCode
        {
            get
            {
                return this.fareBasisCodeField;
            }
            set
            {
                this.fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return this.paxSegmentRefIDField;
            }
            set
            {
                this.paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return this.prefLevelField;
            }
            set
            {
                this.prefLevelField = value;
            }
        }

        /// <remarks/>
        public string PTC
        {
            get
            {
                return this.pTCField;
            }
            set
            {
                this.pTCField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FareCriteriaType
    {

        private string[] fareBasisCodeField;

        private string[] fareSubTypeCodeField;

        private string[] fareTypeCodeField;

        private GroupFareCriteriaType[] groupFareCriteriaField;

        private bool noAdvancePurchaseIndField;

        private bool noAdvancePurchaseIndFieldSpecified;

        private bool noPenaltyIndField;

        private bool noPenaltyIndFieldSpecified;

        private PrefLevelType prefLevelField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareBasisCode", DataType = "token")]
        public string[] FareBasisCode
        {
            get
            {
                return this.fareBasisCodeField;
            }
            set
            {
                this.fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareSubTypeCode", DataType = "token")]
        public string[] FareSubTypeCode
        {
            get
            {
                return this.fareSubTypeCodeField;
            }
            set
            {
                this.fareSubTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareTypeCode", DataType = "token")]
        public string[] FareTypeCode
        {
            get
            {
                return this.fareTypeCodeField;
            }
            set
            {
                this.fareTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("GroupFareCriteria")]
        public GroupFareCriteriaType[] GroupFareCriteria
        {
            get
            {
                return this.groupFareCriteriaField;
            }
            set
            {
                this.groupFareCriteriaField = value;
            }
        }

        /// <remarks/>
        public bool NoAdvancePurchaseInd
        {
            get
            {
                return this.noAdvancePurchaseIndField;
            }
            set
            {
                this.noAdvancePurchaseIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NoAdvancePurchaseIndSpecified
        {
            get
            {
                return this.noAdvancePurchaseIndFieldSpecified;
            }
            set
            {
                this.noAdvancePurchaseIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool NoPenaltyInd
        {
            get
            {
                return this.noPenaltyIndField;
            }
            set
            {
                this.noPenaltyIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NoPenaltyIndSpecified
        {
            get
            {
                return this.noPenaltyIndFieldSpecified;
            }
            set
            {
                this.noPenaltyIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return this.prefLevelField;
            }
            set
            {
                this.prefLevelField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BaggagePricingCriteriaType
    {

        private string baggageOptionCodeField;

        private string commercialAgreementIDField;

        private bool deferralIndField;

        private bool deferralIndFieldSpecified;

        private bool fixedPrePaidIndField;

        private bool fixedPrePaidIndFieldSpecified;

        private bool includeSettlementIndField;

        private bool includeSettlementIndFieldSpecified;

        private string optionalChargesCodeField;

        private string requestedActionCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string BaggageOptionCode
        {
            get
            {
                return this.baggageOptionCodeField;
            }
            set
            {
                this.baggageOptionCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CommercialAgreementID
        {
            get
            {
                return this.commercialAgreementIDField;
            }
            set
            {
                this.commercialAgreementIDField = value;
            }
        }

        /// <remarks/>
        public bool DeferralInd
        {
            get
            {
                return this.deferralIndField;
            }
            set
            {
                this.deferralIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DeferralIndSpecified
        {
            get
            {
                return this.deferralIndFieldSpecified;
            }
            set
            {
                this.deferralIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool FixedPrePaidInd
        {
            get
            {
                return this.fixedPrePaidIndField;
            }
            set
            {
                this.fixedPrePaidIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FixedPrePaidIndSpecified
        {
            get
            {
                return this.fixedPrePaidIndFieldSpecified;
            }
            set
            {
                this.fixedPrePaidIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool IncludeSettlementInd
        {
            get
            {
                return this.includeSettlementIndField;
            }
            set
            {
                this.includeSettlementIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IncludeSettlementIndSpecified
        {
            get
            {
                return this.includeSettlementIndFieldSpecified;
            }
            set
            {
                this.includeSettlementIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OptionalChargesCode
        {
            get
            {
                return this.optionalChargesCodeField;
            }
            set
            {
                this.optionalChargesCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string RequestedActionCode
        {
            get
            {
                return this.requestedActionCodeField;
            }
            set
            {
                this.requestedActionCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferCriteriaType
    {

        private BaggagePricingCriteriaType baggagePricingCriteriaField;

        private FareCriteriaType[] fareCriteriaField;

        private PricingMethodCriteriaType[] pricingMethodCriteriaField;

        private ProgramCriteriaType[] programCriteriaField;

        private PromotionType[] promotionCriteriaField;

        private SeatCriteriaType[] seatCriteriaField;

        private ServiceCriteriaType[] serviceCriteriaField;

        private SpecialServiceType[] specialNeedsCriteriaField;

        private TripPurposeCodeType tripPurposeCodeField;

        private bool tripPurposeCodeFieldSpecified;

        private PrefLevelType waitListCriteriaField;

        /// <remarks/>
        public BaggagePricingCriteriaType BaggagePricingCriteria
        {
            get
            {
                return this.baggagePricingCriteriaField;
            }
            set
            {
                this.baggagePricingCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareCriteria")]
        public FareCriteriaType[] FareCriteria
        {
            get
            {
                return this.fareCriteriaField;
            }
            set
            {
                this.fareCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PricingMethodCriteria")]
        public PricingMethodCriteriaType[] PricingMethodCriteria
        {
            get
            {
                return this.pricingMethodCriteriaField;
            }
            set
            {
                this.pricingMethodCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ProgramCriteria")]
        public ProgramCriteriaType[] ProgramCriteria
        {
            get
            {
                return this.programCriteriaField;
            }
            set
            {
                this.programCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PromotionCriteria")]
        public PromotionType[] PromotionCriteria
        {
            get
            {
                return this.promotionCriteriaField;
            }
            set
            {
                this.promotionCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SeatCriteria")]
        public SeatCriteriaType[] SeatCriteria
        {
            get
            {
                return this.seatCriteriaField;
            }
            set
            {
                this.seatCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ServiceCriteria")]
        public ServiceCriteriaType[] ServiceCriteria
        {
            get
            {
                return this.serviceCriteriaField;
            }
            set
            {
                this.serviceCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SpecialNeedsCriteria")]
        public SpecialServiceType[] SpecialNeedsCriteria
        {
            get
            {
                return this.specialNeedsCriteriaField;
            }
            set
            {
                this.specialNeedsCriteriaField = value;
            }
        }

        /// <remarks/>
        public TripPurposeCodeType TripPurposeCode
        {
            get
            {
                return this.tripPurposeCodeField;
            }
            set
            {
                this.tripPurposeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TripPurposeCodeSpecified
        {
            get
            {
                return this.tripPurposeCodeFieldSpecified;
            }
            set
            {
                this.tripPurposeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PrefLevelType WaitListCriteria
        {
            get
            {
                return this.waitListCriteriaField;
            }
            set
            {
                this.waitListCriteriaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SpecialServiceType
    {

        private MeasureType addlDataMeasureField;

        private string freeTextField;

        private decimal qtyField;

        private bool qtyFieldSpecified;

        private string specialServiceCodeField;

        /// <remarks/>
        public MeasureType AddlDataMeasure
        {
            get
            {
                return this.addlDataMeasureField;
            }
            set
            {
                this.addlDataMeasureField = value;
            }
        }

        /// <remarks/>
        public string FreeText
        {
            get
            {
                return this.freeTextField;
            }
            set
            {
                this.freeTextField = value;
            }
        }

        /// <remarks/>
        public decimal Qty
        {
            get
            {
                return this.qtyField;
            }
            set
            {
                this.qtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool QtySpecified
        {
            get
            {
                return this.qtyFieldSpecified;
            }
            set
            {
                this.qtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SpecialServiceCode
        {
            get
            {
                return this.specialServiceCodeField;
            }
            set
            {
                this.specialServiceCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MeasureType
    {

        private string unitCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
        public string UnitCode
        {
            get
            {
                return this.unitCodeField;
            }
            set
            {
                this.unitCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TripPurposeCodeType
    {

        /// <remarks/>
        Business,

        /// <remarks/>
        BusinessAndLeisure,

        /// <remarks/>
        Leisure,

        /// <remarks/>
        Other,

        /// <remarks/>
        Unknown,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FieldMetadataType
    {

        private bool mandatoryIndField;

        private bool mandatoryIndFieldSpecified;

        private string pathTextField;

        /// <remarks/>
        public bool MandatoryInd
        {
            get
            {
                return this.mandatoryIndField;
            }
            set
            {
                this.mandatoryIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MandatoryIndSpecified
        {
            get
            {
                return this.mandatoryIndFieldSpecified;
            }
            set
            {
                this.mandatoryIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string PathText
        {
            get
            {
                return this.pathTextField;
            }
            set
            {
                this.pathTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CodesetMetadataType
    {

        private string codesetNameField;

        private decimal codesetVersionNumberField;

        private bool codesetVersionNumberFieldSpecified;

        private LangUsageType langUsageField;

        private string ownerIDField;

        private string ownerNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CodesetName
        {
            get
            {
                return this.codesetNameField;
            }
            set
            {
                this.codesetNameField = value;
            }
        }

        /// <remarks/>
        public decimal CodesetVersionNumber
        {
            get
            {
                return this.codesetVersionNumberField;
            }
            set
            {
                this.codesetVersionNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CodesetVersionNumberSpecified
        {
            get
            {
                return this.codesetVersionNumberFieldSpecified;
            }
            set
            {
                this.codesetVersionNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public LangUsageType LangUsage
        {
            get
            {
                return this.langUsageField;
            }
            set
            {
                this.langUsageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerID
        {
            get
            {
                return this.ownerIDField;
            }
            set
            {
                this.ownerIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return this.ownerNameField;
            }
            set
            {
                this.ownerNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MetadataType
    {

        private CodesetMetadataType[] codesetMetadataField;

        private FieldMetadataType[] fieldMetadataField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CodesetMetadata")]
        public CodesetMetadataType[] CodesetMetadata
        {
            get
            {
                return this.codesetMetadataField;
            }
            set
            {
                this.codesetMetadataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FieldMetadata")]
        public FieldMetadataType[] FieldMetadata
        {
            get
            {
                return this.fieldMetadataField;
            }
            set
            {
                this.fieldMetadataField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PenaltyRefType
    {

        private string penaltyRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PenaltyRefID
        {
            get
            {
                return this.penaltyRefIDField;
            }
            set
            {
                this.penaltyRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OrderingQtyType
    {

        private decimal maximumQtyField;

        private bool maximumQtyFieldSpecified;

        private decimal minimumQtyField;

        private bool minimumQtyFieldSpecified;

        /// <remarks/>
        public decimal MaximumQty
        {
            get
            {
                return this.maximumQtyField;
            }
            set
            {
                this.maximumQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaximumQtySpecified
        {
            get
            {
                return this.maximumQtyFieldSpecified;
            }
            set
            {
                this.maximumQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal MinimumQty
        {
            get
            {
                return this.minimumQtyField;
            }
            set
            {
                this.minimumQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinimumQtySpecified
        {
            get
            {
                return this.minimumQtyFieldSpecified;
            }
            set
            {
                this.minimumQtyFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AvailPeriodType
    {

        private System.DateTime earliestAvailablePeriodDateTimeField;

        private bool earliestAvailablePeriodDateTimeFieldSpecified;

        private System.DateTime latestAvailablePeriodDateTimeField;

        private bool latestAvailablePeriodDateTimeFieldSpecified;

        /// <remarks/>
        public System.DateTime EarliestAvailablePeriodDateTime
        {
            get
            {
                return this.earliestAvailablePeriodDateTimeField;
            }
            set
            {
                this.earliestAvailablePeriodDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EarliestAvailablePeriodDateTimeSpecified
        {
            get
            {
                return this.earliestAvailablePeriodDateTimeFieldSpecified;
            }
            set
            {
                this.earliestAvailablePeriodDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime LatestAvailablePeriodDateTime
        {
            get
            {
                return this.latestAvailablePeriodDateTimeField;
            }
            set
            {
                this.latestAvailablePeriodDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LatestAvailablePeriodDateTimeSpecified
        {
            get
            {
                return this.latestAvailablePeriodDateTimeFieldSpecified;
            }
            set
            {
                this.latestAvailablePeriodDateTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TermType
    {

        private AvailPeriodType availPeriodField;

        private DescType descField;

        private OrderingQtyType orderingQtyField;

        private string termIDField;

        /// <remarks/>
        public AvailPeriodType AvailPeriod
        {
            get
            {
                return this.availPeriodField;
            }
            set
            {
                this.availPeriodField = value;
            }
        }

        /// <remarks/>
        public DescType Desc
        {
            get
            {
                return this.descField;
            }
            set
            {
                this.descField = value;
            }
        }

        /// <remarks/>
        public OrderingQtyType OrderingQty
        {
            get
            {
                return this.orderingQtyField;
            }
            set
            {
                this.orderingQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TermID
        {
            get
            {
                return this.termIDField;
            }
            set
            {
                this.termIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LegCabinAssociationsType
    {

        private LegsAssociatedCabinType[] legsAssociatedCabinField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LegsAssociatedCabin")]
        public LegsAssociatedCabinType[] LegsAssociatedCabin
        {
            get
            {
                return this.legsAssociatedCabinField;
            }
            set
            {
                this.legsAssociatedCabinField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LegsAssociatedCabinType
    {

        private string[] datedOperatingLegRefIDField;

        private CabinTypeType legCabinTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return this.datedOperatingLegRefIDField;
            }
            set
            {
                this.datedOperatingLegRefIDField = value;
            }
        }

        /// <remarks/>
        public CabinTypeType LegCabinType
        {
            get
            {
                return this.legCabinTypeField;
            }
            set
            {
                this.legCabinTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CabinTypeAssociationChoiceType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LegCabinAssociations", typeof(LegCabinAssociationsType))]
        [System.Xml.Serialization.XmlElementAttribute("SegmentCabinType", typeof(CabinTypeType))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ShoppingRequestPaxSegmentType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private CabinTypeAssociationChoiceType cabinTypeAssociationChoiceField;

        private string datedMarketingSegmentRefIdField;

        private string[] fareBasisCodeField;

        private string paxSegmentIDField;

        private MediaResourceRefType[] richMediaField;

        private SettlementInfoType settlementInfoField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return this.airlineTaxonomyField;
            }
            set
            {
                this.airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public CabinTypeAssociationChoiceType CabinTypeAssociationChoice
        {
            get
            {
                return this.cabinTypeAssociationChoiceField;
            }
            set
            {
                this.cabinTypeAssociationChoiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string DatedMarketingSegmentRefId
        {
            get
            {
                return this.datedMarketingSegmentRefIdField;
            }
            set
            {
                this.datedMarketingSegmentRefIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("FareBasisCode", DataType = "token")]
        public string[] FareBasisCode
        {
            get
            {
                return this.fareBasisCodeField;
            }
            set
            {
                this.fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxSegmentID
        {
            get
            {
                return this.paxSegmentIDField;
            }
            set
            {
                this.paxSegmentIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return this.richMediaField;
            }
            set
            {
                this.richMediaField = value;
            }
        }

        /// <remarks/>
        public SettlementInfoType SettlementInfo
        {
            get
            {
                return this.settlementInfoField;
            }
            set
            {
                this.settlementInfoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AirlineTaxonomyType
    {

        private string descTextField;

        private string taxonomyCodeField;

        private TaxonomyFeatureType[] taxonomyFeatureField;

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TaxonomyCode
        {
            get
            {
                return this.taxonomyCodeField;
            }
            set
            {
                this.taxonomyCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TaxonomyFeature")]
        public TaxonomyFeatureType[] TaxonomyFeature
        {
            get
            {
                return this.taxonomyFeatureField;
            }
            set
            {
                this.taxonomyFeatureField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TaxonomyFeatureType
    {

        private string codesetCodeField;

        private string codesetNameCodeField;

        private string valueTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CodesetCode
        {
            get
            {
                return this.codesetCodeField;
            }
            set
            {
                this.codesetCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CodesetNameCode
        {
            get
            {
                return this.codesetNameCodeField;
            }
            set
            {
                this.codesetNameCodeField = value;
            }
        }

        /// <remarks/>
        public string ValueText
        {
            get
            {
                return this.valueTextField;
            }
            set
            {
                this.valueTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MediaResourceRefType
    {

        private string contentTypeField;

        private string descTextField;

        private string resourceURIField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ContentType
        {
            get
            {
                return this.contentTypeField;
            }
            set
            {
                this.contentTypeField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string ResourceURI
        {
            get
            {
                return this.resourceURIField;
            }
            set
            {
                this.resourceURIField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SettlementInfoType
    {

        private CommissionType[] commissionField;

        private FeeType[] feeField;

        private string methodCodeField;

        private AmountType settlementAmountField;

        private SurchargeType[] surchargeField;

        private TaxType[] taxField;

        private bool taxableIndField;

        private bool taxableIndFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Commission")]
        public CommissionType[] Commission
        {
            get
            {
                return this.commissionField;
            }
            set
            {
                this.commissionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Fee")]
        public FeeType[] Fee
        {
            get
            {
                return this.feeField;
            }
            set
            {
                this.feeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string MethodCode
        {
            get
            {
                return this.methodCodeField;
            }
            set
            {
                this.methodCodeField = value;
            }
        }

        /// <remarks/>
        public AmountType SettlementAmount
        {
            get
            {
                return this.settlementAmountField;
            }
            set
            {
                this.settlementAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Surcharge")]
        public SurchargeType[] Surcharge
        {
            get
            {
                return this.surchargeField;
            }
            set
            {
                this.surchargeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Tax")]
        public TaxType[] Tax
        {
            get
            {
                return this.taxField;
            }
            set
            {
                this.taxField = value;
            }
        }

        /// <remarks/>
        public bool TaxableInd
        {
            get
            {
                return this.taxableIndField;
            }
            set
            {
                this.taxableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TaxableIndSpecified
        {
            get
            {
                return this.taxableIndFieldSpecified;
            }
            set
            {
                this.taxableIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CommissionType
    {

        private AmountType amountField;

        private string commissionCodeField;

        private decimal percentageField;

        private bool percentageFieldSpecified;

        private AmountType percentageAppliedToAmountField;

        private bool taxableIndField;

        private bool taxableIndFieldSpecified;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CommissionCode
        {
            get
            {
                return this.commissionCodeField;
            }
            set
            {
                this.commissionCodeField = value;
            }
        }

        /// <remarks/>
        public decimal Percentage
        {
            get
            {
                return this.percentageField;
            }
            set
            {
                this.percentageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PercentageSpecified
        {
            get
            {
                return this.percentageFieldSpecified;
            }
            set
            {
                this.percentageFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType PercentageAppliedToAmount
        {
            get
            {
                return this.percentageAppliedToAmountField;
            }
            set
            {
                this.percentageAppliedToAmountField = value;
            }
        }

        /// <remarks/>
        public bool TaxableInd
        {
            get
            {
                return this.taxableIndField;
            }
            set
            {
                this.taxableIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TaxableIndSpecified
        {
            get
            {
                return this.taxableIndFieldSpecified;
            }
            set
            {
                this.taxableIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceBundleType
    {

        private decimal maximumServiceQtyField;

        private bool maximumServiceQtyFieldSpecified;

        private string[] serviceDefinitionRefIDField;

        /// <remarks/>
        public decimal MaximumServiceQty
        {
            get
            {
                return this.maximumServiceQtyField;
            }
            set
            {
                this.maximumServiceQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaximumServiceQtySpecified
        {
            get
            {
                return this.maximumServiceQtyFieldSpecified;
            }
            set
            {
                this.maximumServiceQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ServiceDefinitionRefID", DataType = "token")]
        public string[] ServiceDefinitionRefID
        {
            get
            {
                return this.serviceDefinitionRefIDField;
            }
            set
            {
                this.serviceDefinitionRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SeatProfileRefType
    {

        private string[] seatProfileRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SeatProfileRefID", DataType = "token")]
        public string[] SeatProfileRefID
        {
            get
            {
                return this.seatProfileRefIDField;
            }
            set
            {
                this.seatProfileRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BaggageAllowanceRefType
    {

        private string[] baggageAllowanceRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BaggageAllowanceRefID", DataType = "token")]
        public string[] BaggageAllowanceRefID
        {
            get
            {
                return this.baggageAllowanceRefIDField;
            }
            set
            {
                this.baggageAllowanceRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceDefinitionAssociationType
    {

        private object itemField;

        private ItemChoiceType itemElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BaggageAllowanceRef", typeof(BaggageAllowanceRefType))]
        [System.Xml.Serialization.XmlElementAttribute("SeatProfileRef", typeof(SeatProfileRefType))]
        [System.Xml.Serialization.XmlElementAttribute("ServiceBundle", typeof(ServiceBundleType))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemElementName")]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemChoiceType ItemElementName
        {
            get
            {
                return this.itemElementNameField;
            }
            set
            {
                this.itemElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes", IncludeInSchema = false)]
    public enum ItemChoiceType
    {

        /// <remarks/>
        BaggageAllowanceRef,

        /// <remarks/>
        SeatProfileRef,

        /// <remarks/>
        ServiceBundle,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BookingInstructionsType
    {

        private string methodTextField;

        private string[] oSI_TextField;

        private SpecialServiceType[] specialServiceField;

        private string upgradeMethodCodeField;

        private string upgradeNewClassCodeField;

        /// <remarks/>
        public string MethodText
        {
            get
            {
                return this.methodTextField;
            }
            set
            {
                this.methodTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OSI_Text")]
        public string[] OSI_Text
        {
            get
            {
                return this.oSI_TextField;
            }
            set
            {
                this.oSI_TextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SpecialService")]
        public SpecialServiceType[] SpecialService
        {
            get
            {
                return this.specialServiceField;
            }
            set
            {
                this.specialServiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string UpgradeMethodCode
        {
            get
            {
                return this.upgradeMethodCodeField;
            }
            set
            {
                this.upgradeMethodCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string UpgradeNewClassCode
        {
            get
            {
                return this.upgradeNewClassCodeField;
            }
            set
            {
                this.upgradeNewClassCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BilateralTimeLimitType
    {

        private string descTextField;

        private string nameField;

        private System.DateTime timeLimitDateTimeField;

        private bool timeLimitDateTimeFieldSpecified;

        /// <remarks/>
        public string DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public System.DateTime TimeLimitDateTime
        {
            get
            {
                return this.timeLimitDateTimeField;
            }
            set
            {
                this.timeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimeLimitDateTimeSpecified
        {
            get
            {
                return this.timeLimitDateTimeFieldSpecified;
            }
            set
            {
                this.timeLimitDateTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceDefinitionType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private BilateralTimeLimitType[] bilateralTimeLimitField;

        private BookingInstructionsType bookingInstructionsField;

        private System.DateTime depositTimeLimitDateTimeField;

        private bool depositTimeLimitDateTimeFieldSpecified;

        private DescType[] descField;

        private string nameField;

        private System.DateTime namingTimeLimitDateTimeField;

        private bool namingTimeLimitDateTimeFieldSpecified;

        private string ownerCodeField;

        private string rFICField;

        private string rFISCField;

        private string serviceCodeField;

        private ServiceDefinitionAssociationType serviceDefinitionAssociationField;

        private string serviceDefinitionIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return this.airlineTaxonomyField;
            }
            set
            {
                this.airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BilateralTimeLimit")]
        public BilateralTimeLimitType[] BilateralTimeLimit
        {
            get
            {
                return this.bilateralTimeLimitField;
            }
            set
            {
                this.bilateralTimeLimitField = value;
            }
        }

        /// <remarks/>
        public BookingInstructionsType BookingInstructions
        {
            get
            {
                return this.bookingInstructionsField;
            }
            set
            {
                this.bookingInstructionsField = value;
            }
        }

        /// <remarks/>
        public System.DateTime DepositTimeLimitDateTime
        {
            get
            {
                return this.depositTimeLimitDateTimeField;
            }
            set
            {
                this.depositTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DepositTimeLimitDateTimeSpecified
        {
            get
            {
                return this.depositTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                this.depositTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Desc")]
        public DescType[] Desc
        {
            get
            {
                return this.descField;
            }
            set
            {
                this.descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public System.DateTime NamingTimeLimitDateTime
        {
            get
            {
                return this.namingTimeLimitDateTimeField;
            }
            set
            {
                this.namingTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NamingTimeLimitDateTimeSpecified
        {
            get
            {
                return this.namingTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                this.namingTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return this.ownerCodeField;
            }
            set
            {
                this.ownerCodeField = value;
            }
        }

        /// <remarks/>
        public string RFIC
        {
            get
            {
                return this.rFICField;
            }
            set
            {
                this.rFICField = value;
            }
        }

        /// <remarks/>
        public string RFISC
        {
            get
            {
                return this.rFISCField;
            }
            set
            {
                this.rFISCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ServiceCode
        {
            get
            {
                return this.serviceCodeField;
            }
            set
            {
                this.serviceCodeField = value;
            }
        }

        /// <remarks/>
        public ServiceDefinitionAssociationType ServiceDefinitionAssociation
        {
            get
            {
                return this.serviceDefinitionAssociationField;
            }
            set
            {
                this.serviceDefinitionAssociationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ServiceDefinitionID
        {
            get
            {
                return this.serviceDefinitionIDField;
            }
            set
            {
                this.serviceDefinitionIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SeatProfileType
    {

        private DescType[] marketingInfoField;

        private MediaResourceRefType[] richMediaField;

        private string[] seatCharacteristicCodeField;

        private KeyValuePropertyType[] seatKeywordsField;

        private MeasureType seatPitchMeasureField;

        private string seatProfileIDField;

        private MeasureType seatWidthMeasureField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MarketingInfo")]
        public DescType[] MarketingInfo
        {
            get
            {
                return this.marketingInfoField;
            }
            set
            {
                this.marketingInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return this.richMediaField;
            }
            set
            {
                this.richMediaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SeatCharacteristicCode", DataType = "token")]
        public string[] SeatCharacteristicCode
        {
            get
            {
                return this.seatCharacteristicCodeField;
            }
            set
            {
                this.seatCharacteristicCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SeatKeywords")]
        public KeyValuePropertyType[] SeatKeywords
        {
            get
            {
                return this.seatKeywordsField;
            }
            set
            {
                this.seatKeywordsField = value;
            }
        }

        /// <remarks/>
        public MeasureType SeatPitchMeasure
        {
            get
            {
                return this.seatPitchMeasureField;
            }
            set
            {
                this.seatPitchMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SeatProfileID
        {
            get
            {
                return this.seatProfileIDField;
            }
            set
            {
                this.seatProfileIDField = value;
            }
        }

        /// <remarks/>
        public MeasureType SeatWidthMeasure
        {
            get
            {
                return this.seatWidthMeasureField;
            }
            set
            {
                this.seatWidthMeasureField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PriceClassType
    {

        private CabinTypeType[] cabinTypeField;

        private string codeField;

        private DescType[] descField;

        private string displayOrderTextField;

        private string fareBasisAppTextField;

        private string fareBasisCodeField;

        private string nameField;

        private string priceClassIDField;

        private MediaResourceRefType[] richMediaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CabinType")]
        public CabinTypeType[] CabinType
        {
            get
            {
                return this.cabinTypeField;
            }
            set
            {
                this.cabinTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Code
        {
            get
            {
                return this.codeField;
            }
            set
            {
                this.codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Desc")]
        public DescType[] Desc
        {
            get
            {
                return this.descField;
            }
            set
            {
                this.descField = value;
            }
        }

        /// <remarks/>
        public string DisplayOrderText
        {
            get
            {
                return this.displayOrderTextField;
            }
            set
            {
                this.displayOrderTextField = value;
            }
        }

        /// <remarks/>
        public string FareBasisAppText
        {
            get
            {
                return this.fareBasisAppTextField;
            }
            set
            {
                this.fareBasisAppTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FareBasisCode
        {
            get
            {
                return this.fareBasisCodeField;
            }
            set
            {
                this.fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PriceClassID
        {
            get
            {
                return this.priceClassIDField;
            }
            set
            {
                this.priceClassIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return this.richMediaField;
            }
            set
            {
                this.richMediaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PenaltyPriceType
    {

        private AmountType baseAmountField;

        private TaxSummaryType[] taxSummaryField;

        private AmountType totalAmountField;

        /// <remarks/>
        public AmountType BaseAmount
        {
            get
            {
                return this.baseAmountField;
            }
            set
            {
                this.baseAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TaxSummary")]
        public TaxSummaryType[] TaxSummary
        {
            get
            {
                return this.taxSummaryField;
            }
            set
            {
                this.taxSummaryField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalAmount
        {
            get
            {
                return this.totalAmountField;
            }
            set
            {
                this.totalAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PenaltyType
    {

        private string[] descTextField;

        private bool netIndField;

        private bool netIndFieldSpecified;

        private string penaltyIDField;

        private PenaltyPriceType priceField;

        private PenaltyTypeCodeType typeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DescText")]
        public string[] DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public bool NetInd
        {
            get
            {
                return this.netIndField;
            }
            set
            {
                this.netIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NetIndSpecified
        {
            get
            {
                return this.netIndFieldSpecified;
            }
            set
            {
                this.netIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PenaltyID
        {
            get
            {
                return this.penaltyIDField;
            }
            set
            {
                this.penaltyIDField = value;
            }
        }

        /// <remarks/>
        public PenaltyPriceType Price
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
        public PenaltyTypeCodeType TypeCode
        {
            get
            {
                return this.typeCodeField;
            }
            set
            {
                this.typeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum PenaltyTypeCodeType
    {

        /// <remarks/>
        Cancellation,

        /// <remarks/>
        Change,

        /// <remarks/>
        NoShow,

        /// <remarks/>
        Other,

        /// <remarks/>
        Upgrade,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RedressCaseType
    {

        private string countryCodeField;

        private string programNameField;

        private string redressCaseIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return this.countryCodeField;
            }
            set
            {
                this.countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ProgramName
        {
            get
            {
                return this.programNameField;
            }
            set
            {
                this.programNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string RedressCaseID
        {
            get
            {
                return this.redressCaseIDField;
            }
            set
            {
                this.redressCaseIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AllianceType
    {

        private string allianceCodeField;

        private CarrierType[] carrierField;

        private string nameField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AllianceCode
        {
            get
            {
                return this.allianceCodeField;
            }
            set
            {
                this.allianceCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Carrier")]
        public CarrierType[] Carrier
        {
            get
            {
                return this.carrierField;
            }
            set
            {
                this.carrierField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return this.uRLField;
            }
            set
            {
                this.uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LoyaltyProgramType
    {

        private AllianceType allianceField;

        private CarrierType carrierField;

        private string programCodeField;

        private string programNameField;

        private string providerNameField;

        private string uRLField;

        /// <remarks/>
        public AllianceType Alliance
        {
            get
            {
                return this.allianceField;
            }
            set
            {
                this.allianceField = value;
            }
        }

        /// <remarks/>
        public CarrierType Carrier
        {
            get
            {
                return this.carrierField;
            }
            set
            {
                this.carrierField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ProgramCode
        {
            get
            {
                return this.programCodeField;
            }
            set
            {
                this.programCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ProgramName
        {
            get
            {
                return this.programNameField;
            }
            set
            {
                this.programNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ProviderName
        {
            get
            {
                return this.providerNameField;
            }
            set
            {
                this.providerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return this.uRLField;
            }
            set
            {
                this.uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LoyaltyProgramAccountType
    {

        private string accountNumberField;

        private LoyaltyProgramType loyaltyProgramField;

        private string signInIDField;

        private string tierCodeField;

        private string tierNameField;

        private string tierPriorityTextField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AccountNumber
        {
            get
            {
                return this.accountNumberField;
            }
            set
            {
                this.accountNumberField = value;
            }
        }

        /// <remarks/>
        public LoyaltyProgramType LoyaltyProgram
        {
            get
            {
                return this.loyaltyProgramField;
            }
            set
            {
                this.loyaltyProgramField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SignInID
        {
            get
            {
                return this.signInIDField;
            }
            set
            {
                this.signInIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TierCode
        {
            get
            {
                return this.tierCodeField;
            }
            set
            {
                this.tierCodeField = value;
            }
        }

        /// <remarks/>
        public string TierName
        {
            get
            {
                return this.tierNameField;
            }
            set
            {
                this.tierNameField = value;
            }
        }

        /// <remarks/>
        public string TierPriorityText
        {
            get
            {
                return this.tierPriorityTextField;
            }
            set
            {
                this.tierPriorityTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return this.uRLField;
            }
            set
            {
                this.uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class VisaType
    {

        private decimal entryQtyField;

        private bool entryQtyFieldSpecified;

        private System.DateTime expiryDateField;

        private bool expiryDateFieldSpecified;

        private string hostCountryCodeField;

        private System.DateTime issueDateField;

        private bool issueDateFieldSpecified;

        private string issuingCountryCodeField;

        private string stayDurationField;

        private string visaIDField;

        private string visaTypeCodeField;

        /// <remarks/>
        public decimal EntryQty
        {
            get
            {
                return this.entryQtyField;
            }
            set
            {
                this.entryQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EntryQtySpecified
        {
            get
            {
                return this.entryQtyFieldSpecified;
            }
            set
            {
                this.entryQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime ExpiryDate
        {
            get
            {
                return this.expiryDateField;
            }
            set
            {
                this.expiryDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpiryDateSpecified
        {
            get
            {
                return this.expiryDateFieldSpecified;
            }
            set
            {
                this.expiryDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string HostCountryCode
        {
            get
            {
                return this.hostCountryCodeField;
            }
            set
            {
                this.hostCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime IssueDate
        {
            get
            {
                return this.issueDateField;
            }
            set
            {
                this.issueDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssueDateSpecified
        {
            get
            {
                return this.issueDateFieldSpecified;
            }
            set
            {
                this.issueDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IssuingCountryCode
        {
            get
            {
                return this.issuingCountryCodeField;
            }
            set
            {
                this.issuingCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string StayDuration
        {
            get
            {
                return this.stayDurationField;
            }
            set
            {
                this.stayDurationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string VisaID
        {
            get
            {
                return this.visaIDField;
            }
            set
            {
                this.visaIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string VisaTypeCode
        {
            get
            {
                return this.visaTypeCodeField;
            }
            set
            {
                this.visaTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IdentityDocType
    {

        private AddlNameType[] addlNameField;

        private System.DateTime birthdateField;

        private bool birthdateFieldSpecified;

        private string birthplaceTextField;

        private string citizenshipCountryCodeField;

        private System.DateTime expiryDateField;

        private bool expiryDateFieldSpecified;

        private GenderCodeType genderCodeField;

        private bool genderCodeFieldSpecified;

        private string[] givenNameField;

        private string identityDocIDField;

        private string identityDocTypeCodeField;

        private System.DateTime issueDateField;

        private bool issueDateFieldSpecified;

        private string issuingCountryCodeField;

        private string[] middleNameField;

        private string residenceCountryCodeField;

        private string suffixNameField;

        private string surnameField;

        private string titleNameField;

        private VisaType[] visaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AddlName")]
        public AddlNameType[] AddlName
        {
            get
            {
                return this.addlNameField;
            }
            set
            {
                this.addlNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime Birthdate
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BirthdateSpecified
        {
            get
            {
                return this.birthdateFieldSpecified;
            }
            set
            {
                this.birthdateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string BirthplaceText
        {
            get
            {
                return this.birthplaceTextField;
            }
            set
            {
                this.birthplaceTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CitizenshipCountryCode
        {
            get
            {
                return this.citizenshipCountryCodeField;
            }
            set
            {
                this.citizenshipCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime ExpiryDate
        {
            get
            {
                return this.expiryDateField;
            }
            set
            {
                this.expiryDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpiryDateSpecified
        {
            get
            {
                return this.expiryDateFieldSpecified;
            }
            set
            {
                this.expiryDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public GenderCodeType GenderCode
        {
            get
            {
                return this.genderCodeField;
            }
            set
            {
                this.genderCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GenderCodeSpecified
        {
            get
            {
                return this.genderCodeFieldSpecified;
            }
            set
            {
                this.genderCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("GivenName", DataType = "token")]
        public string[] GivenName
        {
            get
            {
                return this.givenNameField;
            }
            set
            {
                this.givenNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IdentityDocID
        {
            get
            {
                return this.identityDocIDField;
            }
            set
            {
                this.identityDocIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IdentityDocTypeCode
        {
            get
            {
                return this.identityDocTypeCodeField;
            }
            set
            {
                this.identityDocTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime IssueDate
        {
            get
            {
                return this.issueDateField;
            }
            set
            {
                this.issueDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IssueDateSpecified
        {
            get
            {
                return this.issueDateFieldSpecified;
            }
            set
            {
                this.issueDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IssuingCountryCode
        {
            get
            {
                return this.issuingCountryCodeField;
            }
            set
            {
                this.issuingCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MiddleName", DataType = "token")]
        public string[] MiddleName
        {
            get
            {
                return this.middleNameField;
            }
            set
            {
                this.middleNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ResidenceCountryCode
        {
            get
            {
                return this.residenceCountryCodeField;
            }
            set
            {
                this.residenceCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SuffixName
        {
            get
            {
                return this.suffixNameField;
            }
            set
            {
                this.suffixNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Surname
        {
            get
            {
                return this.surnameField;
            }
            set
            {
                this.surnameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TitleName
        {
            get
            {
                return this.titleNameField;
            }
            set
            {
                this.titleNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Visa")]
        public VisaType[] Visa
        {
            get
            {
                return this.visaField;
            }
            set
            {
                this.visaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FOID_Type
    {

        private string fOID_IDField;

        private string fOID_TypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FOID_ID
        {
            get
            {
                return this.fOID_IDField;
            }
            set
            {
                this.fOID_IDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string FOID_TypeCode
        {
            get
            {
                return this.fOID_TypeCodeField;
            }
            set
            {
                this.fOID_TypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SalesTaxRegistrationType
    {

        private string cityNameField;

        private string countryCodeField;

        private string countryDialingCodeField;

        private string countrySubDivisionNameField;

        private string emailAddressTextField;

        private string[] phoneNumberField;

        private string postalCodeField;

        private string salesTaxRegistrationIDField;

        private string[] streetTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CityName
        {
            get
            {
                return this.cityNameField;
            }
            set
            {
                this.cityNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryCode
        {
            get
            {
                return this.countryCodeField;
            }
            set
            {
                this.countryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountryDialingCode
        {
            get
            {
                return this.countryDialingCodeField;
            }
            set
            {
                this.countryDialingCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CountrySubDivisionName
        {
            get
            {
                return this.countrySubDivisionNameField;
            }
            set
            {
                this.countrySubDivisionNameField = value;
            }
        }

        /// <remarks/>
        public string EmailAddressText
        {
            get
            {
                return this.emailAddressTextField;
            }
            set
            {
                this.emailAddressTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PhoneNumber")]
        public string[] PhoneNumber
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
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PostalCode
        {
            get
            {
                return this.postalCodeField;
            }
            set
            {
                this.postalCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SalesTaxRegistrationID
        {
            get
            {
                return this.salesTaxRegistrationIDField;
            }
            set
            {
                this.salesTaxRegistrationIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StreetText")]
        public string[] StreetText
        {
            get
            {
                return this.streetTextField;
            }
            set
            {
                this.streetTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class EmployerType
    {

        private string nameField;

        private SalesTaxRegistrationType[] salesTaxRegistrationField;

        private TaxExemptionQualificationCodeType taxationExemptionQualifierCodeField;

        private bool taxationExemptionQualifierCodeFieldSpecified;

        private bool taxDeductedatSourceCapabilityIndField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SalesTaxRegistration")]
        public SalesTaxRegistrationType[] SalesTaxRegistration
        {
            get
            {
                return this.salesTaxRegistrationField;
            }
            set
            {
                this.salesTaxRegistrationField = value;
            }
        }

        /// <remarks/>
        public TaxExemptionQualificationCodeType TaxationExemptionQualifierCode
        {
            get
            {
                return this.taxationExemptionQualifierCodeField;
            }
            set
            {
                this.taxationExemptionQualifierCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TaxationExemptionQualifierCodeSpecified
        {
            get
            {
                return this.taxationExemptionQualifierCodeFieldSpecified;
            }
            set
            {
                this.taxationExemptionQualifierCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool TaxDeductedatSourceCapabilityInd
        {
            get
            {
                return this.taxDeductedatSourceCapabilityIndField;
            }
            set
            {
                this.taxDeductedatSourceCapabilityIndField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TaxExemptionQualificationCodeType
    {

        /// <remarks/>
        PSU,

        /// <remarks/>
        SEZ,

        /// <remarks/>
        UN,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxType
    {

        private MeasureType ageMeasureField;

        private System.DateTime birthdateField;

        private bool birthdateFieldSpecified;

        private string citizenshipCountryCodeField;

        private string contactInfoRefIDField;

        private EmployerType employerField;

        private FOID_Type fOIDField;

        private IdentityDocType[] identityDocField;

        private IndividualType individualField;

        private LangUsageType[] langUsageField;

        private LoyaltyProgramAccountType[] loyaltyProgramAccountField;

        private string paxIDField;

        private string paxRefIDField;

        private bool profileConsentIndField;

        private bool profileConsentIndFieldSpecified;

        private string profileID_TextField;

        private string pTCField;

        private RedressCaseType[] redressCaseField;

        private RemarkType[] remarkField;

        private string residenceCountryCodeField;

        /// <remarks/>
        public MeasureType AgeMeasure
        {
            get
            {
                return this.ageMeasureField;
            }
            set
            {
                this.ageMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime Birthdate
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BirthdateSpecified
        {
            get
            {
                return this.birthdateFieldSpecified;
            }
            set
            {
                this.birthdateFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CitizenshipCountryCode
        {
            get
            {
                return this.citizenshipCountryCodeField;
            }
            set
            {
                this.citizenshipCountryCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ContactInfoRefID
        {
            get
            {
                return this.contactInfoRefIDField;
            }
            set
            {
                this.contactInfoRefIDField = value;
            }
        }

        /// <remarks/>
        public EmployerType Employer
        {
            get
            {
                return this.employerField;
            }
            set
            {
                this.employerField = value;
            }
        }

        /// <remarks/>
        public FOID_Type FOID
        {
            get
            {
                return this.fOIDField;
            }
            set
            {
                this.fOIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IdentityDoc")]
        public IdentityDocType[] IdentityDoc
        {
            get
            {
                return this.identityDocField;
            }
            set
            {
                this.identityDocField = value;
            }
        }

        /// <remarks/>
        public IndividualType Individual
        {
            get
            {
                return this.individualField;
            }
            set
            {
                this.individualField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LangUsage")]
        public LangUsageType[] LangUsage
        {
            get
            {
                return this.langUsageField;
            }
            set
            {
                this.langUsageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LoyaltyProgramAccount")]
        public LoyaltyProgramAccountType[] LoyaltyProgramAccount
        {
            get
            {
                return this.loyaltyProgramAccountField;
            }
            set
            {
                this.loyaltyProgramAccountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxID
        {
            get
            {
                return this.paxIDField;
            }
            set
            {
                this.paxIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return this.paxRefIDField;
            }
            set
            {
                this.paxRefIDField = value;
            }
        }

        /// <remarks/>
        public bool ProfileConsentInd
        {
            get
            {
                return this.profileConsentIndField;
            }
            set
            {
                this.profileConsentIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ProfileConsentIndSpecified
        {
            get
            {
                return this.profileConsentIndFieldSpecified;
            }
            set
            {
                this.profileConsentIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string ProfileID_Text
        {
            get
            {
                return this.profileID_TextField;
            }
            set
            {
                this.profileID_TextField = value;
            }
        }

        /// <remarks/>
        public string PTC
        {
            get
            {
                return this.pTCField;
            }
            set
            {
                this.pTCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RedressCase")]
        public RedressCaseType[] RedressCase
        {
            get
            {
                return this.redressCaseField;
            }
            set
            {
                this.redressCaseField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return this.remarkField;
            }
            set
            {
                this.remarkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string ResidenceCountryCode
        {
            get
            {
                return this.residenceCountryCodeField;
            }
            set
            {
                this.residenceCountryCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxJourneyType
    {

        private MeasureType distanceMeasureField;

        private string durationField;

        private string paxJourneyIDField;

        private string[] paxSegmentRefIDField;

        private SettlementInfoType settlementInfoField;

        /// <remarks/>
        public MeasureType DistanceMeasure
        {
            get
            {
                return this.distanceMeasureField;
            }
            set
            {
                this.distanceMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string Duration
        {
            get
            {
                return this.durationField;
            }
            set
            {
                this.durationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PaxJourneyID
        {
            get
            {
                return this.paxJourneyIDField;
            }
            set
            {
                this.paxJourneyIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return this.paxSegmentRefIDField;
            }
            set
            {
                this.paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        public SettlementInfoType SettlementInfo
        {
            get
            {
                return this.settlementInfoField;
            }
            set
            {
                this.settlementInfoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OriginDestType
    {

        private string destCodeField;

        private string originCodeField;

        private string originDestIDField;

        private string[] paxJourneyRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string DestCode
        {
            get
            {
                return this.destCodeField;
            }
            set
            {
                this.destCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OriginCode
        {
            get
            {
                return this.originCodeField;
            }
            set
            {
                this.originCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OriginDestID
        {
            get
            {
                return this.originDestIDField;
            }
            set
            {
                this.originDestIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaxJourneyRefID", DataType = "token")]
        public string[] PaxJourneyRefID
        {
            get
            {
                return this.paxJourneyRefIDField;
            }
            set
            {
                this.paxJourneyRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DisclosureType
    {

        private DescType[] descField;

        private string disclosureIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Desc")]
        public DescType[] Desc
        {
            get
            {
                return this.descField;
            }
            set
            {
                this.descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string DisclosureID
        {
            get
            {
                return this.disclosureIDField;
            }
            set
            {
                this.disclosureIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedOperatingSegmentType
    {

        private string carrierDesigCodeField;

        private string carrierNameField;

        private string[] datedOperatingLegRefIDField;

        private string datedOperatingSegmentIdField;

        private string disclosureRefIDField;

        private string durationField;

        private string operatingCarrierFlightNumberTextField;

        private string operationalSuffixTextField;

        private bool secureFlightIndField;

        private bool secureFlightIndFieldSpecified;

        private string segmentTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CarrierDesigCode
        {
            get
            {
                return this.carrierDesigCodeField;
            }
            set
            {
                this.carrierDesigCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CarrierName
        {
            get
            {
                return this.carrierNameField;
            }
            set
            {
                this.carrierNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return this.datedOperatingLegRefIDField;
            }
            set
            {
                this.datedOperatingLegRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string DatedOperatingSegmentId
        {
            get
            {
                return this.datedOperatingSegmentIdField;
            }
            set
            {
                this.datedOperatingSegmentIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string DisclosureRefID
        {
            get
            {
                return this.disclosureRefIDField;
            }
            set
            {
                this.disclosureRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string Duration
        {
            get
            {
                return this.durationField;
            }
            set
            {
                this.durationField = value;
            }
        }

        /// <remarks/>
        public string OperatingCarrierFlightNumberText
        {
            get
            {
                return this.operatingCarrierFlightNumberTextField;
            }
            set
            {
                this.operatingCarrierFlightNumberTextField = value;
            }
        }

        /// <remarks/>
        public string OperationalSuffixText
        {
            get
            {
                return this.operationalSuffixTextField;
            }
            set
            {
                this.operationalSuffixTextField = value;
            }
        }

        /// <remarks/>
        public bool SecureFlightInd
        {
            get
            {
                return this.secureFlightIndField;
            }
            set
            {
                this.secureFlightIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SecureFlightIndSpecified
        {
            get
            {
                return this.secureFlightIndFieldSpecified;
            }
            set
            {
                this.secureFlightIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SegmentTypeCode
        {
            get
            {
                return this.segmentTypeCodeField;
            }
            set
            {
                this.segmentTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TransportPointType
    {

        private System.DateTime aircraftScheduledDateTimeField;

        private bool aircraftScheduledDateTimeFieldSpecified;

        private string boardingGateIDField;

        private string iATA_LocationCodeField;

        private string stationNameField;

        private string terminalNameField;

        /// <remarks/>
        public System.DateTime AircraftScheduledDateTime
        {
            get
            {
                return this.aircraftScheduledDateTimeField;
            }
            set
            {
                this.aircraftScheduledDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AircraftScheduledDateTimeSpecified
        {
            get
            {
                return this.aircraftScheduledDateTimeFieldSpecified;
            }
            set
            {
                this.aircraftScheduledDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string BoardingGateID
        {
            get
            {
                return this.boardingGateIDField;
            }
            set
            {
                this.boardingGateIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string IATA_LocationCode
        {
            get
            {
                return this.iATA_LocationCodeField;
            }
            set
            {
                this.iATA_LocationCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string StationName
        {
            get
            {
                return this.stationNameField;
            }
            set
            {
                this.stationNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TerminalName
        {
            get
            {
                return this.terminalNameField;
            }
            set
            {
                this.terminalNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedMarketingSegmentType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private TransportPointType arrivalField;

        private string carrierDesigCodeField;

        private string carrierNameField;

        private string datedMarketingSegmentIdField;

        private string datedOperatingSegmentRefIdField;

        private TransportPointType depField;

        private string marketingCarrierFlightNumberTextField;

        private string operationalSuffixTextField;

        private MediaResourceRefType[] richMediaField;

        private bool ticketlessIndField;

        private bool ticketlessIndFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return this.airlineTaxonomyField;
            }
            set
            {
                this.airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public TransportPointType Arrival
        {
            get
            {
                return this.arrivalField;
            }
            set
            {
                this.arrivalField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CarrierDesigCode
        {
            get
            {
                return this.carrierDesigCodeField;
            }
            set
            {
                this.carrierDesigCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CarrierName
        {
            get
            {
                return this.carrierNameField;
            }
            set
            {
                this.carrierNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string DatedMarketingSegmentId
        {
            get
            {
                return this.datedMarketingSegmentIdField;
            }
            set
            {
                this.datedMarketingSegmentIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string DatedOperatingSegmentRefId
        {
            get
            {
                return this.datedOperatingSegmentRefIdField;
            }
            set
            {
                this.datedOperatingSegmentRefIdField = value;
            }
        }

        /// <remarks/>
        public TransportPointType Dep
        {
            get
            {
                return this.depField;
            }
            set
            {
                this.depField = value;
            }
        }

        /// <remarks/>
        public string MarketingCarrierFlightNumberText
        {
            get
            {
                return this.marketingCarrierFlightNumberTextField;
            }
            set
            {
                this.marketingCarrierFlightNumberTextField = value;
            }
        }

        /// <remarks/>
        public string OperationalSuffixText
        {
            get
            {
                return this.operationalSuffixTextField;
            }
            set
            {
                this.operationalSuffixTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return this.richMediaField;
            }
            set
            {
                this.richMediaField = value;
            }
        }

        /// <remarks/>
        public bool TicketlessInd
        {
            get
            {
                return this.ticketlessIndField;
            }
            set
            {
                this.ticketlessIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TicketlessIndSpecified
        {
            get
            {
                return this.ticketlessIndFieldSpecified;
            }
            set
            {
                this.ticketlessIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BaggageDisclosureType
    {

        private string baggageDisclosureIDField;

        private BDC_Type bDCField;

        private bool checkInChargesIndField;

        private bool checkInChargesIndFieldSpecified;

        private string commercialAgreementIDField;

        private bool deferralIndField;

        private bool deferralIndFieldSpecified;

        private DescType[] descField;

        private string[] descTextField;

        private bool fixedPrePaidIndField;

        private bool fixedPrePaidIndFieldSpecified;

        private BagDisclosureRuleTypeCodeType ruleTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string BaggageDisclosureID
        {
            get
            {
                return this.baggageDisclosureIDField;
            }
            set
            {
                this.baggageDisclosureIDField = value;
            }
        }

        /// <remarks/>
        public BDC_Type BDC
        {
            get
            {
                return this.bDCField;
            }
            set
            {
                this.bDCField = value;
            }
        }

        /// <remarks/>
        public bool CheckInChargesInd
        {
            get
            {
                return this.checkInChargesIndField;
            }
            set
            {
                this.checkInChargesIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CheckInChargesIndSpecified
        {
            get
            {
                return this.checkInChargesIndFieldSpecified;
            }
            set
            {
                this.checkInChargesIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CommercialAgreementID
        {
            get
            {
                return this.commercialAgreementIDField;
            }
            set
            {
                this.commercialAgreementIDField = value;
            }
        }

        /// <remarks/>
        public bool DeferralInd
        {
            get
            {
                return this.deferralIndField;
            }
            set
            {
                this.deferralIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DeferralIndSpecified
        {
            get
            {
                return this.deferralIndFieldSpecified;
            }
            set
            {
                this.deferralIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Desc")]
        public DescType[] Desc
        {
            get
            {
                return this.descField;
            }
            set
            {
                this.descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DescText")]
        public string[] DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        public bool FixedPrePaidInd
        {
            get
            {
                return this.fixedPrePaidIndField;
            }
            set
            {
                this.fixedPrePaidIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FixedPrePaidIndSpecified
        {
            get
            {
                return this.fixedPrePaidIndFieldSpecified;
            }
            set
            {
                this.fixedPrePaidIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public BagDisclosureRuleTypeCodeType RuleTypeCode
        {
            get
            {
                return this.ruleTypeCodeField;
            }
            set
            {
                this.ruleTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum BagDisclosureRuleTypeCodeType
    {

        /// <remarks/>
        D,

        /// <remarks/>
        N,

        /// <remarks/>
        Other,

        /// <remarks/>
        Y,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class WeightAllowanceType
    {

        private decimal maximumWeightMeasureField;

        private bool maximumWeightMeasureFieldSpecified;

        private decimal minimumWeightMeasureField;

        private bool minimumWeightMeasureFieldSpecified;

        private decimal totalMaximumWeightMeasureField;

        private bool totalMaximumWeightMeasureFieldSpecified;

        private WeightUnitCodeType weightUnitOfMeasurementField;

        /// <remarks/>
        public decimal MaximumWeightMeasure
        {
            get
            {
                return this.maximumWeightMeasureField;
            }
            set
            {
                this.maximumWeightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaximumWeightMeasureSpecified
        {
            get
            {
                return this.maximumWeightMeasureFieldSpecified;
            }
            set
            {
                this.maximumWeightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal MinimumWeightMeasure
        {
            get
            {
                return this.minimumWeightMeasureField;
            }
            set
            {
                this.minimumWeightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinimumWeightMeasureSpecified
        {
            get
            {
                return this.minimumWeightMeasureFieldSpecified;
            }
            set
            {
                this.minimumWeightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal TotalMaximumWeightMeasure
        {
            get
            {
                return this.totalMaximumWeightMeasureField;
            }
            set
            {
                this.totalMaximumWeightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TotalMaximumWeightMeasureSpecified
        {
            get
            {
                return this.totalMaximumWeightMeasureFieldSpecified;
            }
            set
            {
                this.totalMaximumWeightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public WeightUnitCodeType WeightUnitOfMeasurement
        {
            get
            {
                return this.weightUnitOfMeasurementField;
            }
            set
            {
                this.weightUnitOfMeasurementField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum WeightUnitCodeType
    {

        /// <remarks/>
        KGM,

        /// <remarks/>
        LBR,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PieceAllowanceType
    {

        private decimal totalQtyField;

        /// <remarks/>
        public decimal TotalQty
        {
            get
            {
                return this.totalQtyField;
            }
            set
            {
                this.totalQtyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DimensionAllowanceType
    {

        private decimal heightMeasureField;

        private bool heightMeasureFieldSpecified;

        private decimal lengthMeasureField;

        private bool lengthMeasureFieldSpecified;

        private LengthUnitCodeType lengthUnitOfMeasurementField;

        private decimal linearMeasureField;

        private bool linearMeasureFieldSpecified;

        private decimal widthMeasureField;

        private bool widthMeasureFieldSpecified;

        /// <remarks/>
        public decimal HeightMeasure
        {
            get
            {
                return this.heightMeasureField;
            }
            set
            {
                this.heightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HeightMeasureSpecified
        {
            get
            {
                return this.heightMeasureFieldSpecified;
            }
            set
            {
                this.heightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal LengthMeasure
        {
            get
            {
                return this.lengthMeasureField;
            }
            set
            {
                this.lengthMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LengthMeasureSpecified
        {
            get
            {
                return this.lengthMeasureFieldSpecified;
            }
            set
            {
                this.lengthMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public LengthUnitCodeType LengthUnitOfMeasurement
        {
            get
            {
                return this.lengthUnitOfMeasurementField;
            }
            set
            {
                this.lengthUnitOfMeasurementField = value;
            }
        }

        /// <remarks/>
        public decimal LinearMeasure
        {
            get
            {
                return this.linearMeasureField;
            }
            set
            {
                this.linearMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LinearMeasureSpecified
        {
            get
            {
                return this.linearMeasureFieldSpecified;
            }
            set
            {
                this.linearMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal WidthMeasure
        {
            get
            {
                return this.widthMeasureField;
            }
            set
            {
                this.widthMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool WidthMeasureSpecified
        {
            get
            {
                return this.widthMeasureFieldSpecified;
            }
            set
            {
                this.widthMeasureFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum LengthUnitCodeType
    {

        /// <remarks/>
        CMT,

        /// <remarks/>
        FOT,

        /// <remarks/>
        INH,

        /// <remarks/>
        MTR,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BaggageAllowanceType
    {

        private AirlineTaxonomyType airlineTaxonomyField;

        private string applicablePartyTextField;

        private string baggageAllowanceIDField;

        private BDC_Type bDCField;

        private string[] descTextField;

        private DimensionAllowanceType[] maximumDimensionAllowanceField;

        private DimensionAllowanceType[] minimumDimensionAllowanceField;

        private PieceAllowanceType pieceAllowanceField;

        private string rFISCField;

        private string typeCodeField;

        private WeightAllowanceType[] weightAllowanceField;

        /// <remarks/>
        public AirlineTaxonomyType AirlineTaxonomy
        {
            get
            {
                return this.airlineTaxonomyField;
            }
            set
            {
                this.airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public string ApplicablePartyText
        {
            get
            {
                return this.applicablePartyTextField;
            }
            set
            {
                this.applicablePartyTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string BaggageAllowanceID
        {
            get
            {
                return this.baggageAllowanceIDField;
            }
            set
            {
                this.baggageAllowanceIDField = value;
            }
        }

        /// <remarks/>
        public BDC_Type BDC
        {
            get
            {
                return this.bDCField;
            }
            set
            {
                this.bDCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("DescText")]
        public string[] DescText
        {
            get
            {
                return this.descTextField;
            }
            set
            {
                this.descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MaximumDimensionAllowance")]
        public DimensionAllowanceType[] MaximumDimensionAllowance
        {
            get
            {
                return this.maximumDimensionAllowanceField;
            }
            set
            {
                this.maximumDimensionAllowanceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MinimumDimensionAllowance")]
        public DimensionAllowanceType[] MinimumDimensionAllowance
        {
            get
            {
                return this.minimumDimensionAllowanceField;
            }
            set
            {
                this.minimumDimensionAllowanceField = value;
            }
        }

        /// <remarks/>
        public PieceAllowanceType PieceAllowance
        {
            get
            {
                return this.pieceAllowanceField;
            }
            set
            {
                this.pieceAllowanceField = value;
            }
        }

        /// <remarks/>
        public string RFISC
        {
            get
            {
                return this.rFISCField;
            }
            set
            {
                this.rFISCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return this.typeCodeField;
            }
            set
            {
                this.typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("WeightAllowance")]
        public WeightAllowanceType[] WeightAllowance
        {
            get
            {
                return this.weightAllowanceField;
            }
            set
            {
                this.weightAllowanceField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ShoppingRequestDataListsType
    {

        private BaggageAllowanceType[] baggageAllowanceListField;

        private BaggageDisclosureType[] baggageDisclosureListField;

        private ContactInfoType[] contactInfoListField;

        private DatedMarketingSegmentType[] datedMarketingSegmentListField;

        private DatedOperatingSegmentType[] datedOperatingSegmentListField;

        private DisclosureType[] disclosureListField;

        private OriginDestType[] originDestListField;

        private PaxJourneyType[] paxJourneyListField;

        private PaxType[] paxListField;

        private PenaltyType[] penaltyListField;

        private PriceClassType[] priceClassListField;

        private SeatProfileType[] seatProfileListField;

        private ServiceDefinitionType[] serviceDefinitionListField;

        private ShoppingRequestPaxSegmentType[] shoppingRequestPaxSegmentListField;

        private TermType[] termsListField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("BaggageAllowance", IsNullable = false)]
        public BaggageAllowanceType[] BaggageAllowanceList
        {
            get
            {
                return this.baggageAllowanceListField;
            }
            set
            {
                this.baggageAllowanceListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("BaggageDisclosure", IsNullable = false)]
        public BaggageDisclosureType[] BaggageDisclosureList
        {
            get
            {
                return this.baggageDisclosureListField;
            }
            set
            {
                this.baggageDisclosureListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ContactInfo", IsNullable = false)]
        public ContactInfoType[] ContactInfoList
        {
            get
            {
                return this.contactInfoListField;
            }
            set
            {
                this.contactInfoListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DatedMarketingSegment", IsNullable = false)]
        public DatedMarketingSegmentType[] DatedMarketingSegmentList
        {
            get
            {
                return this.datedMarketingSegmentListField;
            }
            set
            {
                this.datedMarketingSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DatedOperatingSegment", IsNullable = false)]
        public DatedOperatingSegmentType[] DatedOperatingSegmentList
        {
            get
            {
                return this.datedOperatingSegmentListField;
            }
            set
            {
                this.datedOperatingSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Disclosure", IsNullable = false)]
        public DisclosureType[] DisclosureList
        {
            get
            {
                return this.disclosureListField;
            }
            set
            {
                this.disclosureListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("OriginDest", IsNullable = false)]
        public OriginDestType[] OriginDestList
        {
            get
            {
                return this.originDestListField;
            }
            set
            {
                this.originDestListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PaxJourney", IsNullable = false)]
        public PaxJourneyType[] PaxJourneyList
        {
            get
            {
                return this.paxJourneyListField;
            }
            set
            {
                this.paxJourneyListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Pax", IsNullable = false)]
        public PaxType[] PaxList
        {
            get
            {
                return this.paxListField;
            }
            set
            {
                this.paxListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Penalty", IsNullable = false)]
        public PenaltyType[] PenaltyList
        {
            get
            {
                return this.penaltyListField;
            }
            set
            {
                this.penaltyListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PriceClass", IsNullable = false)]
        public PriceClassType[] PriceClassList
        {
            get
            {
                return this.priceClassListField;
            }
            set
            {
                this.priceClassListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("SeatProfile", IsNullable = false)]
        public SeatProfileType[] SeatProfileList
        {
            get
            {
                return this.seatProfileListField;
            }
            set
            {
                this.seatProfileListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ServiceDefinition", IsNullable = false)]
        public ServiceDefinitionType[] ServiceDefinitionList
        {
            get
            {
                return this.serviceDefinitionListField;
            }
            set
            {
                this.serviceDefinitionListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PaxSegment", IsNullable = false)]
        public ShoppingRequestPaxSegmentType[] ShoppingRequestPaxSegmentList
        {
            get
            {
                return this.shoppingRequestPaxSegmentListField;
            }
            set
            {
                this.shoppingRequestPaxSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Term", IsNullable = false)]
        public TermType[] TermsList
        {
            get
            {
                return this.termsListField;
            }
            set
            {
                this.termsListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferPriceRequestType
    {

        private ShoppingRequestDataListsType dataListsField;

        private PenaltyRefType inExchForTicketField;

        private MetadataType metadataField;

        private OfferCriteriaType offerCriteriaField;

        private string[] originDestRefIDField;

        private PaymentAssociatedOfferFunctionsCriteriaType[] paymentFunctionsField;

        private PolicyType[] policyField;

        private PricedOfferType pricedOfferField;

        private ResponseParametersRequestType responseParametersField;

        /// <remarks/>
        public ShoppingRequestDataListsType DataLists
        {
            get
            {
                return this.dataListsField;
            }
            set
            {
                this.dataListsField = value;
            }
        }

        /// <remarks/>
        public PenaltyRefType InExchForTicket
        {
            get
            {
                return this.inExchForTicketField;
            }
            set
            {
                this.inExchForTicketField = value;
            }
        }

        /// <remarks/>
        public MetadataType Metadata
        {
            get
            {
                return this.metadataField;
            }
            set
            {
                this.metadataField = value;
            }
        }

        /// <remarks/>
        public OfferCriteriaType OfferCriteria
        {
            get
            {
                return this.offerCriteriaField;
            }
            set
            {
                this.offerCriteriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OriginDestRefID", DataType = "token")]
        public string[] OriginDestRefID
        {
            get
            {
                return this.originDestRefIDField;
            }
            set
            {
                this.originDestRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PaymentFunctions")]
        public PaymentAssociatedOfferFunctionsCriteriaType[] PaymentFunctions
        {
            get
            {
                return this.paymentFunctionsField;
            }
            set
            {
                this.paymentFunctionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Policy")]
        public PolicyType[] Policy
        {
            get
            {
                return this.policyField;
            }
            set
            {
                this.policyField = value;
            }
        }

        /// <remarks/>
        public PricedOfferType PricedOffer
        {
            get
            {
                return this.pricedOfferField;
            }
            set
            {
                this.pricedOfferField = value;
            }
        }

        /// <remarks/>
        public ResponseParametersRequestType ResponseParameters
        {
            get
            {
                return this.responseParametersField;
            }
            set
            {
                this.responseParametersField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class POS_Type
    {

        private string agentDutyTextField;

        private CityType cityField;

        private CountryType countryField;

        private bool onlineTrxIndField;

        private bool onlineTrxIndFieldSpecified;

        private System.DateTime requestTimeField;

        private bool requestTimeFieldSpecified;

        /// <remarks/>
        public string AgentDutyText
        {
            get
            {
                return this.agentDutyTextField;
            }
            set
            {
                this.agentDutyTextField = value;
            }
        }

        /// <remarks/>
        public CityType City
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
            }
        }

        /// <remarks/>
        public CountryType Country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        public bool OnlineTrxInd
        {
            get
            {
                return this.onlineTrxIndField;
            }
            set
            {
                this.onlineTrxIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OnlineTrxIndSpecified
        {
            get
            {
                return this.onlineTrxIndFieldSpecified;
            }
            set
            {
                this.onlineTrxIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime RequestTime
        {
            get
            {
                return this.requestTimeField;
            }
            set
            {
                this.requestTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RequestTimeSpecified
        {
            get
            {
                return this.requestTimeFieldSpecified;
            }
            set
            {
                this.requestTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IATA_PayloadStandardAttributesType
    {

        private string altLangIDField;

        private string correlationIDField;

        private string primaryLangIDField;

        private bool retransmissionIndField;

        private bool retransmissionIndFieldSpecified;

        private decimal seqNumberField;

        private bool seqNumberFieldSpecified;

        private System.DateTime timestampField;

        private bool timestampFieldSpecified;

        private string trxIDField;

        private decimal versionNumberField;

        private bool versionNumberFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string AltLangID
        {
            get
            {
                return this.altLangIDField;
            }
            set
            {
                this.altLangIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string CorrelationID
        {
            get
            {
                return this.correlationIDField;
            }
            set
            {
                this.correlationIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string PrimaryLangID
        {
            get
            {
                return this.primaryLangIDField;
            }
            set
            {
                this.primaryLangIDField = value;
            }
        }

        /// <remarks/>
        public bool RetransmissionInd
        {
            get
            {
                return this.retransmissionIndField;
            }
            set
            {
                this.retransmissionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RetransmissionIndSpecified
        {
            get
            {
                return this.retransmissionIndFieldSpecified;
            }
            set
            {
                this.retransmissionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal SeqNumber
        {
            get
            {
                return this.seqNumberField;
            }
            set
            {
                this.seqNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SeqNumberSpecified
        {
            get
            {
                return this.seqNumberFieldSpecified;
            }
            set
            {
                this.seqNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime Timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimestampSpecified
        {
            get
            {
                return this.timestampFieldSpecified;
            }
            set
            {
                this.timestampFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string TrxID
        {
            get
            {
                return this.trxIDField;
            }
            set
            {
                this.trxIDField = value;
            }
        }

        /// <remarks/>
        public decimal VersionNumber
        {
            get
            {
                return this.versionNumberField;
            }
            set
            {
                this.versionNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VersionNumberSpecified
        {
            get
            {
                return this.versionNumberFieldSpecified;
            }
            set
            {
                this.versionNumberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SalesBranchType
    {

        private string salesBranchIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SalesBranchID
        {
            get
            {
                return this.salesBranchIDField;
            }
            set
            {
                this.salesBranchIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SalesAgentType
    {

        private string salesAgentIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string SalesAgentID
        {
            get
            {
                return this.salesAgentIDField;
            }
            set
            {
                this.salesAgentIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ParticipatingOrgType
    {

        private string nameField;

        private string orgIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string OrgID
        {
            get
            {
                return this.orgIDField;
            }
            set
            {
                this.orgIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DistributionChainLinkType
    {

        private ContactInfoType contactInfoField;

        private string ordinalField;

        private DistributionChainRoleCodeType orgRoleField;

        private ParticipatingOrgType participatingOrgField;

        private SalesAgentType salesAgentField;

        private SalesBranchType salesBranchField;

        /// <remarks/>
        public ContactInfoType ContactInfo
        {
            get
            {
                return this.contactInfoField;
            }
            set
            {
                this.contactInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string Ordinal
        {
            get
            {
                return this.ordinalField;
            }
            set
            {
                this.ordinalField = value;
            }
        }

        /// <remarks/>
        public DistributionChainRoleCodeType OrgRole
        {
            get
            {
                return this.orgRoleField;
            }
            set
            {
                this.orgRoleField = value;
            }
        }

        /// <remarks/>
        public ParticipatingOrgType ParticipatingOrg
        {
            get
            {
                return this.participatingOrgField;
            }
            set
            {
                this.participatingOrgField = value;
            }
        }

        /// <remarks/>
        public SalesAgentType SalesAgent
        {
            get
            {
                return this.salesAgentField;
            }
            set
            {
                this.salesAgentField = value;
            }
        }

        /// <remarks/>
        public SalesBranchType SalesBranch
        {
            get
            {
                return this.salesBranchField;
            }
            set
            {
                this.salesBranchField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum DistributionChainRoleCodeType
    {

        /// <remarks/>
        Carrier,

        /// <remarks/>
        Distributor,

        /// <remarks/>
        Seller,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignatureType
    {

        private SignedInfoType signedInfoField;

        private SignatureValueType signatureValueField;

        private KeyInfoType keyInfoField;

        private ObjectType[] objectField;

        private string idField;

        /// <remarks/>
        public SignedInfoType SignedInfo
        {
            get
            {
                return this.signedInfoField;
            }
            set
            {
                this.signedInfoField = value;
            }
        }

        /// <remarks/>
        public SignatureValueType SignatureValue
        {
            get
            {
                return this.signatureValueField;
            }
            set
            {
                this.signatureValueField = value;
            }
        }

        /// <remarks/>
        public KeyInfoType KeyInfo
        {
            get
            {
                return this.keyInfoField;
            }
            set
            {
                this.keyInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Object")]
        public ObjectType[] Object
        {
            get
            {
                return this.objectField;
            }
            set
            {
                this.objectField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("SignedInfo", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignedInfoType
    {

        private CanonicalizationMethodType canonicalizationMethodField;

        private SignatureMethodType signatureMethodField;

        private ReferenceType[] referenceField;

        private string idField;

        /// <remarks/>
        public CanonicalizationMethodType CanonicalizationMethod
        {
            get
            {
                return this.canonicalizationMethodField;
            }
            set
            {
                this.canonicalizationMethodField = value;
            }
        }

        /// <remarks/>
        public SignatureMethodType SignatureMethod
        {
            get
            {
                return this.signatureMethodField;
            }
            set
            {
                this.signatureMethodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Reference")]
        public ReferenceType[] Reference
        {
            get
            {
                return this.referenceField;
            }
            set
            {
                this.referenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("CanonicalizationMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class CanonicalizationMethodType
    {

        private System.Xml.XmlNode[] anyField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return this.anyField;
            }
            set
            {
                this.anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("SignatureMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignatureMethodType
    {

        private string hMACOutputLengthField;

        private System.Xml.XmlNode[] anyField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string HMACOutputLength
        {
            get
            {
                return this.hMACOutputLengthField;
            }
            set
            {
                this.hMACOutputLengthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return this.anyField;
            }
            set
            {
                this.anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("Reference", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class ReferenceType
    {

        private TransformType[] transformsField;

        private DigestMethodType digestMethodField;

        private byte[] digestValueField;

        private string idField;

        private string uRIField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Transform", IsNullable = false)]
        public TransformType[] Transforms
        {
            get
            {
                return this.transformsField;
            }
            set
            {
                this.transformsField = value;
            }
        }

        /// <remarks/>
        public DigestMethodType DigestMethod
        {
            get
            {
                return this.digestMethodField;
            }
            set
            {
                this.digestMethodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] DigestValue
        {
            get
            {
                return this.digestValueField;
            }
            set
            {
                this.digestValueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string URI
        {
            get
            {
                return this.uRIField;
            }
            set
            {
                this.uRIField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("Transform", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class TransformType
    {

        private object[] itemsField;

        private string[] textField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        [System.Xml.Serialization.XmlElementAttribute("XPath", typeof(string))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("DigestMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class DigestMethodType
    {

        private System.Xml.XmlNode[] anyField;

        private string algorithmField;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return this.anyField;
            }
            set
            {
                this.anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("SignatureValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignatureValueType
    {

        private string idField;

        private byte[] valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType = "base64Binary")]
        public byte[] Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("KeyInfo", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class KeyInfoType
    {

        private object[] itemsField;

        private ItemsChoiceType2[] itemsElementNameField;

        private string[] textField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        [System.Xml.Serialization.XmlElementAttribute("KeyName", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("KeyValue", typeof(KeyValueType))]
        [System.Xml.Serialization.XmlElementAttribute("MgmtData", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("PGPData", typeof(PGPDataType))]
        [System.Xml.Serialization.XmlElementAttribute("RetrievalMethod", typeof(RetrievalMethodType))]
        [System.Xml.Serialization.XmlElementAttribute("SPKIData", typeof(SPKIDataType))]
        [System.Xml.Serialization.XmlElementAttribute("X509Data", typeof(X509DataType))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType2[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("KeyValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class KeyValueType
    {

        private object itemField;

        private string[] textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        [System.Xml.Serialization.XmlElementAttribute("DSAKeyValue", typeof(DSAKeyValueType))]
        [System.Xml.Serialization.XmlElementAttribute("RSAKeyValue", typeof(RSAKeyValueType))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("DSAKeyValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class DSAKeyValueType
    {

        private byte[] pField;

        private byte[] qField;

        private byte[] gField;

        private byte[] yField;

        private byte[] jField;

        private byte[] seedField;

        private byte[] pgenCounterField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] P
        {
            get
            {
                return this.pField;
            }
            set
            {
                this.pField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] Q
        {
            get
            {
                return this.qField;
            }
            set
            {
                this.qField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] G
        {
            get
            {
                return this.gField;
            }
            set
            {
                this.gField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] J
        {
            get
            {
                return this.jField;
            }
            set
            {
                this.jField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] Seed
        {
            get
            {
                return this.seedField;
            }
            set
            {
                this.seedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] PgenCounter
        {
            get
            {
                return this.pgenCounterField;
            }
            set
            {
                this.pgenCounterField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("RSAKeyValue", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class RSAKeyValueType
    {

        private byte[] modulusField;

        private byte[] exponentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] Modulus
        {
            get
            {
                return this.modulusField;
            }
            set
            {
                this.modulusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] Exponent
        {
            get
            {
                return this.exponentField;
            }
            set
            {
                this.exponentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("PGPData", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class PGPDataType
    {

        private object[] itemsField;

        private ItemsChoiceType1[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        [System.Xml.Serialization.XmlElementAttribute("PGPKeyID", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElementAttribute("PGPKeyPacket", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType1[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#", IncludeInSchema = false)]
    public enum ItemsChoiceType1
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("##any:")]
        Item,

        /// <remarks/>
        PGPKeyID,

        /// <remarks/>
        PGPKeyPacket,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("RetrievalMethod", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class RetrievalMethodType
    {

        private TransformType[] transformsField;

        private string uRIField;

        private string typeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Transform", IsNullable = false)]
        public TransformType[] Transforms
        {
            get
            {
                return this.transformsField;
            }
            set
            {
                this.transformsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string URI
        {
            get
            {
                return this.uRIField;
            }
            set
            {
                this.uRIField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
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
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("SPKIData", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SPKIDataType
    {

        private byte[] sPKISexpField;

        private System.Xml.XmlElement anyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SPKISexp", DataType = "base64Binary")]
        public byte[] SPKISexp
        {
            get
            {
                return this.sPKISexpField;
            }
            set
            {
                this.sPKISexpField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlElement Any
        {
            get
            {
                return this.anyField;
            }
            set
            {
                this.anyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("X509Data", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class X509DataType
    {

        private object[] itemsField;

        private ItemsChoiceType[] itemsElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        [System.Xml.Serialization.XmlElementAttribute("X509CRL", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElementAttribute("X509Certificate", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElementAttribute("X509IssuerSerial", typeof(X509IssuerSerialType))]
        [System.Xml.Serialization.XmlElementAttribute("X509SKI", typeof(byte[]), DataType = "base64Binary")]
        [System.Xml.Serialization.XmlElementAttribute("X509SubjectName", typeof(string))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemsChoiceType[] ItemsElementName
        {
            get
            {
                return this.itemsElementNameField;
            }
            set
            {
                this.itemsElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class X509IssuerSerialType
    {

        private string x509IssuerNameField;

        private string x509SerialNumberField;

        /// <remarks/>
        public string X509IssuerName
        {
            get
            {
                return this.x509IssuerNameField;
            }
            set
            {
                this.x509IssuerNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string X509SerialNumber
        {
            get
            {
                return this.x509SerialNumberField;
            }
            set
            {
                this.x509SerialNumberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#", IncludeInSchema = false)]
    public enum ItemsChoiceType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("##any:")]
        Item,

        /// <remarks/>
        X509CRL,

        /// <remarks/>
        X509Certificate,

        /// <remarks/>
        X509IssuerSerial,

        /// <remarks/>
        X509SKI,

        /// <remarks/>
        X509SubjectName,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#", IncludeInSchema = false)]
    public enum ItemsChoiceType2
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("##any:")]
        Item,

        /// <remarks/>
        KeyName,

        /// <remarks/>
        KeyValue,

        /// <remarks/>
        MgmtData,

        /// <remarks/>
        PGPData,

        /// <remarks/>
        RetrievalMethod,

        /// <remarks/>
        SPKIData,

        /// <remarks/>
        X509Data,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("Object", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class ObjectType
    {

        private System.Xml.XmlNode[] anyField;

        private string idField;

        private string mimeTypeField;

        private string encodingField;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlNode[] Any
        {
            get
            {
                return this.anyField;
            }
            set
            {
                this.anyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MimeType
        {
            get
            {
                return this.mimeTypeField;
            }
            set
            {
                this.mimeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Encoding
        {
            get
            {
                return this.encodingField;
            }
            set
            {
                this.encodingField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("Transforms", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class TransformsType
    {

        private TransformType[] transformField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Transform")]
        public TransformType[] Transform
        {
            get
            {
                return this.transformField;
            }
            set
            {
                this.transformField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("Manifest", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class ManifestType
    {

        private ReferenceType[] referenceField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Reference")]
        public ReferenceType[] Reference
        {
            get
            {
                return this.referenceField;
            }
            set
            {
                this.referenceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("SignatureProperties", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignaturePropertiesType
    {

        private SignaturePropertyType[] signaturePropertyField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SignatureProperty")]
        public SignaturePropertyType[] SignatureProperty
        {
            get
            {
                return this.signaturePropertyField;
            }
            set
            {
                this.signaturePropertyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [System.Xml.Serialization.XmlRootAttribute("SignatureProperty", Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class SignaturePropertyType
    {

        private System.Xml.XmlElement[] itemsField;

        private string[] textField;

        private string targetField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlElement[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Target
        {
            get
            {
                return this.targetField;
            }
            set
            {
                this.targetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }
}