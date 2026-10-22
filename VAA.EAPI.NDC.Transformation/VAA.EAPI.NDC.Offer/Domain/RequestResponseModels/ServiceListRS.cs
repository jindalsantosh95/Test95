namespace VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage", IsNullable = false)]
    public partial class IATA_ServiceListRS
    {

        private object[] itemsField;

        private AugmentationPointType augmentationPointField;

        private IATA_PayloadStandardAttributesType payloadAttributesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Error", typeof(ErrorType))]
        [System.Xml.Serialization.XmlElement("Response", typeof(ServiceListResponseType))]
        public object[] Items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;
            }
        }

        /// <remarks/>
        public AugmentationPointType AugmentationPoint
        {
            get
            {
                return augmentationPointField;
            }
            set
            {
                augmentationPointField = value;
            }
        }

        /// <remarks/>
        public IATA_PayloadStandardAttributesType PayloadAttributes
        {
            get
            {
                return payloadAttributesField;
            }
            set
            {
                payloadAttributesField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ErrorType
    {

        private string codeField;

        private string descTextField;

        private string errorIDField;

        private string langCodeField;

        private string ownerNameField;

        private string statusTextField;

        private string tagTextField;

        private string typeCodeField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ErrorID
        {
            get
            {
                return errorIDField;
            }
            set
            {
                errorIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string LangCode
        {
            get
            {
                return langCodeField;
            }
            set
            {
                langCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return ownerNameField;
            }
            set
            {
                ownerNameField = value;
            }
        }

        /// <remarks/>
        public string StatusText
        {
            get
            {
                return statusTextField;
            }
            set
            {
                statusTextField = value;
            }
        }

        /// <remarks/>
        public string TagText
        {
            get
            {
                return tagTextField;
            }
            set
            {
                tagTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return uRLField;
            }
            set
            {
                uRLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceListResponseType
    {

        private ALaCarteOfferType aLaCarteOfferField;

        private DataListsType dataListsField;

        private MetadataType metadataField;

        private PaymentFunctionsOfferType[] paymentFunctionsField;

        private ResponseParametersResponseType processingField;

        private WarningType[] warningField;

        /// <remarks/>
        public ALaCarteOfferType ALaCarteOffer
        {
            get
            {
                return aLaCarteOfferField;
            }
            set
            {
                aLaCarteOfferField = value;
            }
        }

        /// <remarks/>
        public DataListsType DataLists
        {
            get
            {
                return dataListsField;
            }
            set
            {
                dataListsField = value;
            }
        }

        /// <remarks/>
        public MetadataType Metadata
        {
            get
            {
                return metadataField;
            }
            set
            {
                metadataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaymentFunctions")]
        public PaymentFunctionsOfferType[] PaymentFunctions
        {
            get
            {
                return paymentFunctionsField;
            }
            set
            {
                paymentFunctionsField = value;
            }
        }

        /// <remarks/>
        public ResponseParametersResponseType Processing
        {
            get
            {
                return processingField;
            }
            set
            {
                processingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Warning")]
        public WarningType[] Warning
        {
            get
            {
                return warningField;
            }
            set
            {
                warningField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ALaCarteOfferType
    {

        private string[] baggageDisclosureRefIDField;

        private CommissionType[] commissionField;

        private DescType[] descField;

        private string disclosureRefIDField;

        private string matchAppTextField;

        private decimal matchPercentField;

        private bool matchPercentFieldSpecified;

        private MatchTypeCodeType matchTypeCodeField;

        private bool matchTypeCodeFieldSpecified;

        private System.DateTime offerExpirationTimeLimitDateTimeField;

        private bool offerExpirationTimeLimitDateTimeFieldSpecified;

        private string offerIDField;

        private ALaCarteOfferItemType[] offerItemField;

        private string ownerCodeField;

        private string[] penaltyRefIDField;

        private PTC_OfferParametersType[] pTC_OfferParametersField;

        private bool redemptionIndField;

        private bool redemptionIndFieldSpecified;

        private bool requestedDateIndField;

        private bool requestedDateIndFieldSpecified;

        private MediaResourceRefType[] richMediaField;

        private PriceType totalPriceField;

        private string validatingCarrierCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("BaggageDisclosureRefID", DataType = "token")]
        public string[] BaggageDisclosureRefID
        {
            get
            {
                return baggageDisclosureRefIDField;
            }
            set
            {
                baggageDisclosureRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Commission")]
        public CommissionType[] Commission
        {
            get
            {
                return commissionField;
            }
            set
            {
                commissionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Desc")]
        public DescType[] Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DisclosureRefID
        {
            get
            {
                return disclosureRefIDField;
            }
            set
            {
                disclosureRefIDField = value;
            }
        }

        /// <remarks/>
        public string MatchAppText
        {
            get
            {
                return matchAppTextField;
            }
            set
            {
                matchAppTextField = value;
            }
        }

        /// <remarks/>
        public decimal MatchPercent
        {
            get
            {
                return matchPercentField;
            }
            set
            {
                matchPercentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MatchPercentSpecified
        {
            get
            {
                return matchPercentFieldSpecified;
            }
            set
            {
                matchPercentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public MatchTypeCodeType MatchTypeCode
        {
            get
            {
                return matchTypeCodeField;
            }
            set
            {
                matchTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MatchTypeCodeSpecified
        {
            get
            {
                return matchTypeCodeFieldSpecified;
            }
            set
            {
                matchTypeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime OfferExpirationTimeLimitDateTime
        {
            get
            {
                return offerExpirationTimeLimitDateTimeField;
            }
            set
            {
                offerExpirationTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool OfferExpirationTimeLimitDateTimeSpecified
        {
            get
            {
                return offerExpirationTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                offerExpirationTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OfferID
        {
            get
            {
                return offerIDField;
            }
            set
            {
                offerIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OfferItem")]
        public ALaCarteOfferItemType[] OfferItem
        {
            get
            {
                return offerItemField;
            }
            set
            {
                offerItemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PenaltyRefID", DataType = "token")]
        public string[] PenaltyRefID
        {
            get
            {
                return penaltyRefIDField;
            }
            set
            {
                penaltyRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PTC_OfferParameters")]
        public PTC_OfferParametersType[] PTC_OfferParameters
        {
            get
            {
                return pTC_OfferParametersField;
            }
            set
            {
                pTC_OfferParametersField = value;
            }
        }

        /// <remarks/>
        public bool RedemptionInd
        {
            get
            {
                return redemptionIndField;
            }
            set
            {
                redemptionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RedemptionIndSpecified
        {
            get
            {
                return redemptionIndFieldSpecified;
            }
            set
            {
                redemptionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool RequestedDateInd
        {
            get
            {
                return requestedDateIndField;
            }
            set
            {
                requestedDateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RequestedDateIndSpecified
        {
            get
            {
                return requestedDateIndFieldSpecified;
            }
            set
            {
                requestedDateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }

        /// <remarks/>
        public PriceType TotalPrice
        {
            get
            {
                return totalPriceField;
            }
            set
            {
                totalPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ValidatingCarrierCode
        {
            get
            {
                return validatingCarrierCodeField;
            }
            set
            {
                validatingCarrierCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AmountType
    {

        private string curCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
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
        [System.Xml.Serialization.XmlText()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum MatchTypeCodeType
    {

        /// <remarks/>
        Full,

        /// <remarks/>
        None,

        /// <remarks/>
        Other,

        /// <remarks/>
        Partial,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ALaCarteOfferItemType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private CancelRestrictionsType[] cancelRestrictionsField;

        private ChangeRestrictionsType[] changeRestrictionsField;

        private CommissionType[] commissionField;

        private EligibilityType eligibilityField;

        private string offerItemIDField;

        private string offerItemTypeCodeField;

        private string ownerCodeField;

        private PaymentTimeLimitType paymentTimeLimitField;

        private System.DateTime priceGuaranteeTimeLimitDateTimeField;

        private bool priceGuaranteeTimeLimitDateTimeFieldSpecified;

        private MediaResourceRefType[] richMediaField;

        private ALaCarteServiceType serviceField;

        private PriceType unitPriceField;

        private string webAddressURIField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CancelRestrictions")]
        public CancelRestrictionsType[] CancelRestrictions
        {
            get
            {
                return cancelRestrictionsField;
            }
            set
            {
                cancelRestrictionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ChangeRestrictions")]
        public ChangeRestrictionsType[] ChangeRestrictions
        {
            get
            {
                return changeRestrictionsField;
            }
            set
            {
                changeRestrictionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Commission")]
        public CommissionType[] Commission
        {
            get
            {
                return commissionField;
            }
            set
            {
                commissionField = value;
            }
        }

        /// <remarks/>
        public EligibilityType Eligibility
        {
            get
            {
                return eligibilityField;
            }
            set
            {
                eligibilityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OfferItemID
        {
            get
            {
                return offerItemIDField;
            }
            set
            {
                offerItemIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OfferItemTypeCode
        {
            get
            {
                return offerItemTypeCodeField;
            }
            set
            {
                offerItemTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }

        /// <remarks/>
        public PaymentTimeLimitType PaymentTimeLimit
        {
            get
            {
                return paymentTimeLimitField;
            }
            set
            {
                paymentTimeLimitField = value;
            }
        }

        /// <remarks/>
        public System.DateTime PriceGuaranteeTimeLimitDateTime
        {
            get
            {
                return priceGuaranteeTimeLimitDateTimeField;
            }
            set
            {
                priceGuaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PriceGuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return priceGuaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                priceGuaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }

        /// <remarks/>
        public ALaCarteServiceType Service
        {
            get
            {
                return serviceField;
            }
            set
            {
                serviceField = value;
            }
        }

        /// <remarks/>
        public PriceType UnitPrice
        {
            get
            {
                return unitPriceField;
            }
            set
            {
                unitPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string WebAddressURI
        {
            get
            {
                return webAddressURIField;
            }
            set
            {
                webAddressURIField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("TaxonomyFeature")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class TaxonomyFeatureType
    {

        private string codesetCodeField;

        private string codesetNameCodeField;

        private string valueTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return allowedModificationIndField;
            }
            set
            {
                allowedModificationIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AllowedModificationIndSpecified
        {
            get
            {
                return allowedModificationIndFieldSpecified;
            }
            set
            {
                allowedModificationIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime EffectiveDateTime
        {
            get
            {
                return effectiveDateTimeField;
            }
            set
            {
                effectiveDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool EffectiveDateTimeSpecified
        {
            get
            {
                return effectiveDateTimeFieldSpecified;
            }
            set
            {
                effectiveDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime ExpirationDateTime
        {
            get
            {
                return expirationDateTimeField;
            }
            set
            {
                expirationDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ExpirationDateTimeSpecified
        {
            get
            {
                return expirationDateTimeFieldSpecified;
            }
            set
            {
                expirationDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public ChangeOrCancelFeeType Fee
        {
            get
            {
                return feeField;
            }
            set
            {
                feeField = value;
            }
        }

        /// <remarks/>
        public JourneyStageTypeCodeType JourneyStageCode
        {
            get
            {
                return journeyStageCodeField;
            }
            set
            {
                journeyStageCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool JourneyStageCodeSpecified
        {
            get
            {
                return journeyStageCodeFieldSpecified;
            }
            set
            {
                journeyStageCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        public RefundType Refund
        {
            get
            {
                return refundField;
            }
            set
            {
                refundField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return amountField;
            }
            set
            {
                amountField = value;
            }
        }

        /// <remarks/>
        public bool ApproximateInd
        {
            get
            {
                return approximateIndField;
            }
            set
            {
                approximateIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ApproximateIndSpecified
        {
            get
            {
                return approximateIndFieldSpecified;
            }
            set
            {
                approximateIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType MaximumAmount
        {
            get
            {
                return maximumAmountField;
            }
            set
            {
                maximumAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MinimumAmount
        {
            get
            {
                return minimumAmountField;
            }
            set
            {
                minimumAmountField = value;
            }
        }

        /// <remarks/>
        public decimal Percent
        {
            get
            {
                return percentField;
            }
            set
            {
                percentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PercentSpecified
        {
            get
            {
                return percentFieldSpecified;
            }
            set
            {
                percentFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum JourneyStageTypeCodeType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("After Departure")]
        AfterDeparture,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("No Show")]
        NoShow,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("Prior To Departure")]
        PriorToDeparture,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType FixedAmount
        {
            get
            {
                return fixedAmountField;
            }
            set
            {
                fixedAmountField = value;
            }
        }

        /// <remarks/>
        public RefundLevelTypeCodeType LevelTypeCode
        {
            get
            {
                return levelTypeCodeField;
            }
            set
            {
                levelTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool LevelTypeCodeSpecified
        {
            get
            {
                return levelTypeCodeFieldSpecified;
            }
            set
            {
                levelTypeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType MaximumAmount
        {
            get
            {
                return maximumAmountField;
            }
            set
            {
                maximumAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType MinimumAmount
        {
            get
            {
                return minimumAmountField;
            }
            set
            {
                minimumAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaymentTypeCode
        {
            get
            {
                return paymentTypeCodeField;
            }
            set
            {
                paymentTypeCodeField = value;
            }
        }

        /// <remarks/>
        public decimal Percent
        {
            get
            {
                return percentField;
            }
            set
            {
                percentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PercentSpecified
        {
            get
            {
                return percentFieldSpecified;
            }
            set
            {
                percentFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return allowedModificationIndField;
            }
            set
            {
                allowedModificationIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool AllowedModificationIndSpecified
        {
            get
            {
                return allowedModificationIndFieldSpecified;
            }
            set
            {
                allowedModificationIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public ChangeTypeCodeType ChangeTypeCode
        {
            get
            {
                return changeTypeCodeField;
            }
            set
            {
                changeTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ChangeTypeCodeSpecified
        {
            get
            {
                return changeTypeCodeFieldSpecified;
            }
            set
            {
                changeTypeCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public System.DateTime EffectiveDateTime
        {
            get
            {
                return effectiveDateTimeField;
            }
            set
            {
                effectiveDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool EffectiveDateTimeSpecified
        {
            get
            {
                return effectiveDateTimeFieldSpecified;
            }
            set
            {
                effectiveDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime ExpirationDateTime
        {
            get
            {
                return expirationDateTimeField;
            }
            set
            {
                expirationDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ExpirationDateTimeSpecified
        {
            get
            {
                return expirationDateTimeFieldSpecified;
            }
            set
            {
                expirationDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public ChangeOrCancelFeeType Fee
        {
            get
            {
                return feeField;
            }
            set
            {
                feeField = value;
            }
        }

        /// <remarks/>
        public JourneyStageTypeCodeType JourneyStageCode
        {
            get
            {
                return journeyStageCodeField;
            }
            set
            {
                journeyStageCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool JourneyStageCodeSpecified
        {
            get
            {
                return journeyStageCodeFieldSpecified;
            }
            set
            {
                journeyStageCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        public RefundType Refund
        {
            get
            {
                return refundField;
            }
            set
            {
                refundField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class EligibilityType
    {

        private OfferFlightAssociationsType offerFlightAssociationsField;

        private string[] paxRefIDField;

        private string[] priceClassRefIDField;

        /// <remarks/>
        public OfferFlightAssociationsType OfferFlightAssociations
        {
            get
            {
                return offerFlightAssociationsField;
            }
            set
            {
                offerFlightAssociationsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PriceClassRefID", DataType = "token")]
        public string[] PriceClassRefID
        {
            get
            {
                return priceClassRefIDField;
            }
            set
            {
                priceClassRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferFlightAssociationsType
    {

        private object itemField;

        private ItemChoiceType itemElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRef", typeof(DatedOperatingLegRefType))]
        [System.Xml.Serialization.XmlElement("PaxJourneyRef", typeof(PaxJourneyRefType))]
        [System.Xml.Serialization.XmlElement("PaxSegmentReferences", typeof(PaxSegmentRefListType))]
        [System.Xml.Serialization.XmlChoiceIdentifier("ItemElementName")]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public ItemChoiceType ItemElementName
        {
            get
            {
                return itemElementNameField;
            }
            set
            {
                itemElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedOperatingLegRefType
    {

        private string[] datedOperatingLegRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRefID", DataType = "token")]
        public string[] DatedOperatingLegRefID
        {
            get
            {
                return datedOperatingLegRefIDField;
            }
            set
            {
                datedOperatingLegRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxJourneyRefType
    {

        private string[] paxJourneyRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxJourneyRefID", DataType = "token")]
        public string[] PaxJourneyRefID
        {
            get
            {
                return paxJourneyRefIDField;
            }
            set
            {
                paxJourneyRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxSegmentRefListType
    {

        private string[] paxSegmentRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return paxSegmentRefIDField;
            }
            set
            {
                paxSegmentRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes", IncludeInSchema = false)]
    public enum ItemChoiceType
    {

        /// <remarks/>
        DatedOperatingLegRef,

        /// <remarks/>
        PaxJourneyRef,

        /// <remarks/>
        PaxSegmentReferences,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentTimeLimitType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaymentTimeLimitDate", typeof(PaymentTimeLimitDateType))]
        [System.Xml.Serialization.XmlElement("PaymentTimeLimitDuration", typeof(PaymentTimeLimitDurationType))]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentTimeLimitDateType
    {

        private System.DateTime paymentTimeLimitDateTimeField;

        private bool paymentTimeLimitDateTimeFieldSpecified;

        /// <remarks/>
        public System.DateTime PaymentTimeLimitDateTime
        {
            get
            {
                return paymentTimeLimitDateTimeField;
            }
            set
            {
                paymentTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PaymentTimeLimitDateTimeSpecified
        {
            get
            {
                return paymentTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                paymentTimeLimitDateTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentTimeLimitDurationType
    {

        private string paymentTimeLimitDurationField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
        public string PaymentTimeLimitDuration
        {
            get
            {
                return paymentTimeLimitDurationField;
            }
            set
            {
                paymentTimeLimitDurationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MediaResourceRefType
    {

        private string contentTypeField;

        private string descTextField;

        private string resourceURIField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ALaCarteServiceType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private System.DateTime inventoryGuaranteeDateTimeField;

        private bool inventoryGuaranteeDateTimeFieldSpecified;

        private MediaResourceRefType[] richMediaField;

        private string serviceDefinitionRefIDField;

        private string serviceIDField;

        private string serviceRefIDField;

        private CarrierType validatingCarrierField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public System.DateTime InventoryGuaranteeDateTime
        {
            get
            {
                return inventoryGuaranteeDateTimeField;
            }
            set
            {
                inventoryGuaranteeDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool InventoryGuaranteeDateTimeSpecified
        {
            get
            {
                return inventoryGuaranteeDateTimeFieldSpecified;
            }
            set
            {
                inventoryGuaranteeDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ServiceDefinitionRefID
        {
            get
            {
                return serviceDefinitionRefIDField;
            }
            set
            {
                serviceDefinitionRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ServiceID
        {
            get
            {
                return serviceIDField;
            }
            set
            {
                serviceIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ServiceRefID
        {
            get
            {
                return serviceRefIDField;
            }
            set
            {
                serviceRefIDField = value;
            }
        }

        /// <remarks/>
        public CarrierType ValidatingCarrier
        {
            get
            {
                return validatingCarrierField;
            }
            set
            {
                validatingCarrierField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CarrierType
    {

        private string airlineDesigCodeField;

        private bool duplicateDesigIndField;

        private bool duplicateDesigIndFieldSpecified;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return baseAmountField;
            }
            set
            {
                baseAmountField = value;
            }
        }

        /// <remarks/>
        public System.DateTime BaseAmountGuaranteeTimeLimitDateTime
        {
            get
            {
                return baseAmountGuaranteeTimeLimitDateTimeField;
            }
            set
            {
                baseAmountGuaranteeTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool BaseAmountGuaranteeTimeLimitDateTimeSpecified
        {
            get
            {
                return baseAmountGuaranteeTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                baseAmountGuaranteeTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CurConversion")]
        public CurConversionType[] CurConversion
        {
            get
            {
                return curConversionField;
            }
            set
            {
                curConversionField = value;
            }
        }

        /// <remarks/>
        public DiscountType Discount
        {
            get
            {
                return discountField;
            }
            set
            {
                discountField = value;
            }
        }

        /// <remarks/>
        public AmountType EquivAmount
        {
            get
            {
                return equivAmountField;
            }
            set
            {
                equivAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Fee")]
        public FeeType[] Fee
        {
            get
            {
                return feeField;
            }
            set
            {
                feeField = value;
            }
        }

        /// <remarks/>
        public AmountType LoyaltyUnitAmount
        {
            get
            {
                return loyaltyUnitAmountField;
            }
            set
            {
                loyaltyUnitAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string LoyaltyUnitName
        {
            get
            {
                return loyaltyUnitNameField;
            }
            set
            {
                loyaltyUnitNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Markup")]
        public MarkupType[] Markup
        {
            get
            {
                return markupField;
            }
            set
            {
                markupField = value;
            }
        }

        /// <remarks/>
        public bool MaskedInd
        {
            get
            {
                return maskedIndField;
            }
            set
            {
                maskedIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MaskedIndSpecified
        {
            get
            {
                return maskedIndFieldSpecified;
            }
            set
            {
                maskedIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Surcharge")]
        public SurchargeType[] Surcharge
        {
            get
            {
                return surchargeField;
            }
            set
            {
                surchargeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("TaxSummary")]
        public TaxSummaryType[] TaxSummary
        {
            get
            {
                return taxSummaryField;
            }
            set
            {
                taxSummaryField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalAmount
        {
            get
            {
                return totalAmountField;
            }
            set
            {
                totalAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
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
        [System.Xml.Serialization.XmlAttribute()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
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
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
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
        [System.Xml.Serialization.XmlAttribute()]
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
        [System.Xml.Serialization.XmlAttribute()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
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
        [System.Xml.Serialization.XmlText()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return appTextField;
            }
            set
            {
                appTextField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public AmountType DiscountAmount
        {
            get
            {
                return discountAmountField;
            }
            set
            {
                discountAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DiscountContext")]
        public KeyValuePropertyType[] DiscountContext
        {
            get
            {
                return discountContextField;
            }
            set
            {
                discountContextField = value;
            }
        }

        /// <remarks/>
        public decimal DiscountPercent
        {
            get
            {
                return discountPercentField;
            }
            set
            {
                discountPercentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DiscountPercentSpecified
        {
            get
            {
                return discountPercentFieldSpecified;
            }
            set
            {
                discountPercentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType PreDiscountedAmount
        {
            get
            {
                return preDiscountedAmountField;
            }
            set
            {
                preDiscountedAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return keyTextField;
            }
            set
            {
                keyTextField = value;
            }
        }

        /// <remarks/>
        public PrefLevelType PrefLevel
        {
            get
            {
                return prefLevelField;
            }
            set
            {
                prefLevelField = value;
            }
        }

        /// <remarks/>
        public string ValueText
        {
            get
            {
                return valueTextField;
            }
            set
            {
                valueTextField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MarkupType
    {

        private AmountType amountField;

        private string typeCodeField;

        /// <remarks/>
        public AmountType Amount
        {
            get
            {
                return amountField;
            }
            set
            {
                amountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement("Breakdown")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("Tax")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement("CollectionPointTax")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class StationType
    {

        private string iATA_LocationCodeField;

        private string stationNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CountryType
    {

        private string countryCodeField;

        private string countryNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CountrySubDivisionType
    {

        private string countrySubDivisionCodeField;

        private string countrySubDivisionNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum TaxTypeCodeType
    {

        /// <remarks/>
        Applied,

        /// <remarks/>
        Exempt,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PTC_OfferParametersType
    {

        private string[] paxRefIDField;

        private decimal pricedPaxNumberField;

        private bool pricedPaxNumberFieldSpecified;

        private string pTC_PricedCodeField;

        private string pTC_RequestedCodeField;

        private decimal requestedPaxNumberField;

        private bool requestedPaxNumberFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxRefID", DataType = "token")]
        public string[] PaxRefID
        {
            get
            {
                return paxRefIDField;
            }
            set
            {
                paxRefIDField = value;
            }
        }

        /// <remarks/>
        public decimal PricedPaxNumber
        {
            get
            {
                return pricedPaxNumberField;
            }
            set
            {
                pricedPaxNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PricedPaxNumberSpecified
        {
            get
            {
                return pricedPaxNumberFieldSpecified;
            }
            set
            {
                pricedPaxNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string PTC_PricedCode
        {
            get
            {
                return pTC_PricedCodeField;
            }
            set
            {
                pTC_PricedCodeField = value;
            }
        }

        /// <remarks/>
        public string PTC_RequestedCode
        {
            get
            {
                return pTC_RequestedCodeField;
            }
            set
            {
                pTC_RequestedCodeField = value;
            }
        }

        /// <remarks/>
        public decimal RequestedPaxNumber
        {
            get
            {
                return requestedPaxNumberField;
            }
            set
            {
                requestedPaxNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RequestedPaxNumberSpecified
        {
            get
            {
                return requestedPaxNumberFieldSpecified;
            }
            set
            {
                requestedPaxNumberFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DataListsType
    {

        private BaggageAllowanceType[] baggageAllowanceListField;

        private BaggageDisclosureType[] baggageDisclosureListField;

        private ContactInfoType[] contactInfoListField;

        private DatedMarketingSegmentType[] datedMarketingSegmentListField;

        private DatedOperatingLegType[] datedOperatingLegListField;

        private DatedOperatingSegmentType[] datedOperatingSegmentListField;

        private DisclosureType[] disclosureListField;

        private OriginDestType[] originDestListField;

        private PaxJourneyType[] paxJourneyListField;

        private PaxType[] paxListField;

        private PaxSegmentType[] paxSegmentListField;

        private PenaltyType[] penaltyListField;

        private PriceClassType[] priceClassListField;

        private SeatProfileType[] seatProfileListField;

        private ServiceDefinitionType[] serviceDefinitionListField;

        private TermType[] termsListField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("BaggageAllowance", IsNullable = false)]
        public BaggageAllowanceType[] BaggageAllowanceList
        {
            get
            {
                return baggageAllowanceListField;
            }
            set
            {
                baggageAllowanceListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("BaggageDisclosure", IsNullable = false)]
        public BaggageDisclosureType[] BaggageDisclosureList
        {
            get
            {
                return baggageDisclosureListField;
            }
            set
            {
                baggageDisclosureListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("ContactInfo", IsNullable = false)]
        public ContactInfoType[] ContactInfoList
        {
            get
            {
                return contactInfoListField;
            }
            set
            {
                contactInfoListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DatedMarketingSegment", IsNullable = false)]
        public DatedMarketingSegmentType[] DatedMarketingSegmentList
        {
            get
            {
                return datedMarketingSegmentListField;
            }
            set
            {
                datedMarketingSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DatedOperatingLeg", IsNullable = false)]
        public DatedOperatingLegType[] DatedOperatingLegList
        {
            get
            {
                return datedOperatingLegListField;
            }
            set
            {
                datedOperatingLegListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("DatedOperatingSegment", IsNullable = false)]
        public DatedOperatingSegmentType[] DatedOperatingSegmentList
        {
            get
            {
                return datedOperatingSegmentListField;
            }
            set
            {
                datedOperatingSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Disclosure", IsNullable = false)]
        public DisclosureType[] DisclosureList
        {
            get
            {
                return disclosureListField;
            }
            set
            {
                disclosureListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("OriginDest", IsNullable = false)]
        public OriginDestType[] OriginDestList
        {
            get
            {
                return originDestListField;
            }
            set
            {
                originDestListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("PaxJourney", IsNullable = false)]
        public PaxJourneyType[] PaxJourneyList
        {
            get
            {
                return paxJourneyListField;
            }
            set
            {
                paxJourneyListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Pax", IsNullable = false)]
        public PaxType[] PaxList
        {
            get
            {
                return paxListField;
            }
            set
            {
                paxListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("PaxSegment", IsNullable = false)]
        public PaxSegmentType[] PaxSegmentList
        {
            get
            {
                return paxSegmentListField;
            }
            set
            {
                paxSegmentListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Penalty", IsNullable = false)]
        public PenaltyType[] PenaltyList
        {
            get
            {
                return penaltyListField;
            }
            set
            {
                penaltyListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("PriceClass", IsNullable = false)]
        public PriceClassType[] PriceClassList
        {
            get
            {
                return priceClassListField;
            }
            set
            {
                priceClassListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("SeatProfile", IsNullable = false)]
        public SeatProfileType[] SeatProfileList
        {
            get
            {
                return seatProfileListField;
            }
            set
            {
                seatProfileListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("ServiceDefinition", IsNullable = false)]
        public ServiceDefinitionType[] ServiceDefinitionList
        {
            get
            {
                return serviceDefinitionListField;
            }
            set
            {
                serviceDefinitionListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Term", IsNullable = false)]
        public TermType[] TermsList
        {
            get
            {
                return termsListField;
            }
            set
            {
                termsListField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        public string ApplicablePartyText
        {
            get
            {
                return applicablePartyTextField;
            }
            set
            {
                applicablePartyTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string BaggageAllowanceID
        {
            get
            {
                return baggageAllowanceIDField;
            }
            set
            {
                baggageAllowanceIDField = value;
            }
        }

        /// <remarks/>
        public BDC_Type BDC
        {
            get
            {
                return bDCField;
            }
            set
            {
                bDCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DescText")]
        public string[] DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("MaximumDimensionAllowance")]
        public DimensionAllowanceType[] MaximumDimensionAllowance
        {
            get
            {
                return maximumDimensionAllowanceField;
            }
            set
            {
                maximumDimensionAllowanceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("MinimumDimensionAllowance")]
        public DimensionAllowanceType[] MinimumDimensionAllowance
        {
            get
            {
                return minimumDimensionAllowanceField;
            }
            set
            {
                minimumDimensionAllowanceField = value;
            }
        }

        /// <remarks/>
        public PieceAllowanceType PieceAllowance
        {
            get
            {
                return pieceAllowanceField;
            }
            set
            {
                pieceAllowanceField = value;
            }
        }

        /// <remarks/>
        public string RFISC
        {
            get
            {
                return rFISCField;
            }
            set
            {
                rFISCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("WeightAllowance")]
        public WeightAllowanceType[] WeightAllowance
        {
            get
            {
                return weightAllowanceField;
            }
            set
            {
                weightAllowanceField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return heightMeasureField;
            }
            set
            {
                heightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool HeightMeasureSpecified
        {
            get
            {
                return heightMeasureFieldSpecified;
            }
            set
            {
                heightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal LengthMeasure
        {
            get
            {
                return lengthMeasureField;
            }
            set
            {
                lengthMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool LengthMeasureSpecified
        {
            get
            {
                return lengthMeasureFieldSpecified;
            }
            set
            {
                lengthMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public LengthUnitCodeType LengthUnitOfMeasurement
        {
            get
            {
                return lengthUnitOfMeasurementField;
            }
            set
            {
                lengthUnitOfMeasurementField = value;
            }
        }

        /// <remarks/>
        public decimal LinearMeasure
        {
            get
            {
                return linearMeasureField;
            }
            set
            {
                linearMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool LinearMeasureSpecified
        {
            get
            {
                return linearMeasureFieldSpecified;
            }
            set
            {
                linearMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal WidthMeasure
        {
            get
            {
                return widthMeasureField;
            }
            set
            {
                widthMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool WidthMeasureSpecified
        {
            get
            {
                return widthMeasureFieldSpecified;
            }
            set
            {
                widthMeasureFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PieceAllowanceType
    {

        private decimal totalQtyField;

        /// <remarks/>
        public decimal TotalQty
        {
            get
            {
                return totalQtyField;
            }
            set
            {
                totalQtyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return maximumWeightMeasureField;
            }
            set
            {
                maximumWeightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MaximumWeightMeasureSpecified
        {
            get
            {
                return maximumWeightMeasureFieldSpecified;
            }
            set
            {
                maximumWeightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal MinimumWeightMeasure
        {
            get
            {
                return minimumWeightMeasureField;
            }
            set
            {
                minimumWeightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MinimumWeightMeasureSpecified
        {
            get
            {
                return minimumWeightMeasureFieldSpecified;
            }
            set
            {
                minimumWeightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal TotalMaximumWeightMeasure
        {
            get
            {
                return totalMaximumWeightMeasureField;
            }
            set
            {
                totalMaximumWeightMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TotalMaximumWeightMeasureSpecified
        {
            get
            {
                return totalMaximumWeightMeasureFieldSpecified;
            }
            set
            {
                totalMaximumWeightMeasureFieldSpecified = value;
            }
        }

        /// <remarks/>
        public WeightUnitCodeType WeightUnitOfMeasurement
        {
            get
            {
                return weightUnitOfMeasurementField;
            }
            set
            {
                weightUnitOfMeasurementField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum WeightUnitCodeType
    {

        /// <remarks/>
        KGM,

        /// <remarks/>
        LBR,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string BaggageDisclosureID
        {
            get
            {
                return baggageDisclosureIDField;
            }
            set
            {
                baggageDisclosureIDField = value;
            }
        }

        /// <remarks/>
        public BDC_Type BDC
        {
            get
            {
                return bDCField;
            }
            set
            {
                bDCField = value;
            }
        }

        /// <remarks/>
        public bool CheckInChargesInd
        {
            get
            {
                return checkInChargesIndField;
            }
            set
            {
                checkInChargesIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool CheckInChargesIndSpecified
        {
            get
            {
                return checkInChargesIndFieldSpecified;
            }
            set
            {
                checkInChargesIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CommercialAgreementID
        {
            get
            {
                return commercialAgreementIDField;
            }
            set
            {
                commercialAgreementIDField = value;
            }
        }

        /// <remarks/>
        public bool DeferralInd
        {
            get
            {
                return deferralIndField;
            }
            set
            {
                deferralIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DeferralIndSpecified
        {
            get
            {
                return deferralIndFieldSpecified;
            }
            set
            {
                deferralIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Desc")]
        public DescType[] Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DescText")]
        public string[] DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public bool FixedPrePaidInd
        {
            get
            {
                return fixedPrePaidIndField;
            }
            set
            {
                fixedPrePaidIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool FixedPrePaidIndSpecified
        {
            get
            {
                return fixedPrePaidIndFieldSpecified;
            }
            set
            {
                fixedPrePaidIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public BagDisclosureRuleTypeCodeType RuleTypeCode
        {
            get
            {
                return ruleTypeCodeField;
            }
            set
            {
                ruleTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("ContactPurposeText")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement("EmailAddress")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("OtherAddress")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("Phone")]
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
        [System.Xml.Serialization.XmlElement("PostalAddress")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement("AddlName")]
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
        [System.Xml.Serialization.XmlElement(DataType = "date")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement("GivenName", DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("MiddleName", DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AddlNameType
    {

        private string nameField;

        private AddlNameTypeCodeType nameTypeCodeField;

        private bool nameTypeCodeFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum AddlNameTypeCodeType
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("Father Surname")]
        FatherSurname,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnum("Mother Surname")]
        MotherSurname,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("StreetText")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("RichMedia")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DatedOperatingLegType
    {

        private AirlineTaxonomyType[] airlineTaxonomyField;

        private TransportPointType arrivalField;

        private CarrierAircraftTypeType carrierAircraftTypeField;

        private bool changeofGaugeIndField;

        private bool changeofGaugeIndFieldSpecified;

        private string datedOperatingLegIDField;

        private TransportPointType depField;

        private MeasureType distanceMeasureField;

        private IATA_AircraftTypeType iATA_AircraftTypeField;

        private string onGroundDurationField;

        private MediaResourceRefType[] richMediaField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
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
        public CarrierAircraftTypeType CarrierAircraftType
        {
            get
            {
                return this.carrierAircraftTypeField;
            }
            set
            {
                this.carrierAircraftTypeField = value;
            }
        }

        /// <remarks/>
        public bool ChangeofGaugeInd
        {
            get
            {
                return this.changeofGaugeIndField;
            }
            set
            {
                this.changeofGaugeIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ChangeofGaugeIndSpecified
        {
            get
            {
                return this.changeofGaugeIndFieldSpecified;
            }
            set
            {
                this.changeofGaugeIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DatedOperatingLegID
        {
            get
            {
                return this.datedOperatingLegIDField;
            }
            set
            {
                this.datedOperatingLegIDField = value;
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
        public IATA_AircraftTypeType IATA_AircraftType
        {
            get
            {
                return this.iATA_AircraftTypeField;
            }
            set
            {
                this.iATA_AircraftTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
        public string OnGroundDuration
        {
            get
            {
                return this.onGroundDurationField;
            }
            set
            {
                this.onGroundDurationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CarrierAircraftTypeType
    {

        private string carrierAircraftTypeCodeField;

        private string carrierAircraftTypeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierAircraftTypeCode
        {
            get
            {
                return this.carrierAircraftTypeCodeField;
            }
            set
            {
                this.carrierAircraftTypeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CarrierAircraftTypeName
        {
            get
            {
                return this.carrierAircraftTypeNameField;
            }
            set
            {
                this.carrierAircraftTypeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MeasureType
    {

        private string unitCodeField;

        private decimal valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
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
        [System.Xml.Serialization.XmlText()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class IATA_AircraftTypeType
    {

        private string iATA_AircraftTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string IATA_AircraftTypeCode
        {
            get
            {
                return this.iATA_AircraftTypeCodeField;
            }
            set
            {
                this.iATA_AircraftTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRefID", DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class DisclosureType
    {

        private DescType[] descField;

        private string disclosureIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Desc")]
        public DescType[] Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DisclosureID
        {
            get
            {
                return disclosureIDField;
            }
            set
            {
                disclosureIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OriginDestType
    {

        private string destCodeField;

        private string originCodeField;

        private string originDestIDField;

        private string[] paxJourneyRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string DestCode
        {
            get
            {
                return destCodeField;
            }
            set
            {
                destCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OriginCode
        {
            get
            {
                return originCodeField;
            }
            set
            {
                originCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OriginDestID
        {
            get
            {
                return originDestIDField;
            }
            set
            {
                originDestIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxJourneyRefID", DataType = "token")]
        public string[] PaxJourneyRefID
        {
            get
            {
                return paxJourneyRefIDField;
            }
            set
            {
                paxJourneyRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return distanceMeasureField;
            }
            set
            {
                distanceMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
        public string Duration
        {
            get
            {
                return durationField;
            }
            set
            {
                durationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaxJourneyID
        {
            get
            {
                return paxJourneyIDField;
            }
            set
            {
                paxJourneyIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PaxSegmentRefID", DataType = "token")]
        public string[] PaxSegmentRefID
        {
            get
            {
                return paxSegmentRefIDField;
            }
            set
            {
                paxSegmentRefIDField = value;
            }
        }

        /// <remarks/>
        public SettlementInfoType SettlementInfo
        {
            get
            {
                return settlementInfoField;
            }
            set
            {
                settlementInfoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement("Commission")]
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
        [System.Xml.Serialization.XmlElement("Fee")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("Surcharge")]
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
        [System.Xml.Serialization.XmlElement("Tax")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxType
    {

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
        [System.Xml.Serialization.XmlElement(DataType = "date")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("IdentityDoc")]
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
        [System.Xml.Serialization.XmlElement("LangUsage")]
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
        [System.Xml.Serialization.XmlElement("LoyaltyProgramAccount")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement("RedressCase")]
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
        [System.Xml.Serialization.XmlElement("Remark")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class EmployerType
    {

        private string nameField;

        private SalesTaxRegistrationType[] salesTaxRegistrationField;

        private TaxExemptionQualificationCodeType taxationExemptionQualifierCodeField;

        private bool taxationExemptionQualifierCodeFieldSpecified;

        private bool taxDeductedatSourceCapabilityIndField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("SalesTaxRegistration")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("PhoneNumber")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("StreetText")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class FOID_Type
    {

        private string fOID_IDField;

        private string fOID_TypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement("AddlName")]
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
        [System.Xml.Serialization.XmlElement(DataType = "date")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "date")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement("GivenName", DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "date")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("MiddleName", DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("Visa")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "date")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "date")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "duration")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AllianceType
    {

        private string allianceCodeField;

        private CarrierType[] carrierField;

        private string nameField;

        private string uRLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement("Carrier")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RedressCaseType
    {

        private string countryCodeField;

        private string programNameField;

        private string redressCaseIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxSegmentType
    {

        private CabinTypeAssociationChoiceType cabinTypeAssociationChoiceField;

        private string datedMarketingSegmentRefIdField;

        private string marketingCarrierRBD_CodeField;

        private string operatingCarrierRBD_CodeField;

        private string paxSegmentIDField;

        private SettlementInfoType settlementInfoField;

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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string MarketingCarrierRBD_Code
        {
            get
            {
                return this.marketingCarrierRBD_CodeField;
            }
            set
            {
                this.marketingCarrierRBD_CodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OperatingCarrierRBD_Code
        {
            get
            {
                return this.operatingCarrierRBD_CodeField;
            }
            set
            {
                this.operatingCarrierRBD_CodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CabinTypeAssociationChoiceType
    {

        private object itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LegCabinAssociations", typeof(LegCabinAssociationsType))]
        [System.Xml.Serialization.XmlElement("SegmentCabinType", typeof(CabinTypeType))]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LegCabinAssociationsType
    {

        private LegsAssociatedCabinType[] legsAssociatedCabinField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LegsAssociatedCabin")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class LegsAssociatedCabinType
    {

        private string[] datedOperatingLegRefIDField;

        private CabinTypeType legCabinTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DatedOperatingLegRefID", DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CabinTypeType
    {

        private string cabinTypeCodeField;

        private string cabinTypeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PenaltyType
    {

        private string[] descTextField;

        private bool netIndField;

        private bool netIndFieldSpecified;

        private string penaltyIDField;

        private PenaltyPriceType priceField;

        private PenaltyTypeCodeType typeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("DescText")]
        public string[] DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        public bool NetInd
        {
            get
            {
                return netIndField;
            }
            set
            {
                netIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool NetIndSpecified
        {
            get
            {
                return netIndFieldSpecified;
            }
            set
            {
                netIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PenaltyID
        {
            get
            {
                return penaltyIDField;
            }
            set
            {
                penaltyIDField = value;
            }
        }

        /// <remarks/>
        public PenaltyPriceType Price
        {
            get
            {
                return priceField;
            }
            set
            {
                priceField = value;
            }
        }

        /// <remarks/>
        public PenaltyTypeCodeType TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return baseAmountField;
            }
            set
            {
                baseAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("TaxSummary")]
        public TaxSummaryType[] TaxSummary
        {
            get
            {
                return taxSummaryField;
            }
            set
            {
                taxSummaryField = value;
            }
        }

        /// <remarks/>
        public AmountType TotalAmount
        {
            get
            {
                return totalAmountField;
            }
            set
            {
                totalAmountField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement("CabinType")]
        public CabinTypeType[] CabinType
        {
            get
            {
                return cabinTypeField;
            }
            set
            {
                cabinTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Desc")]
        public DescType[] Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        public string DisplayOrderText
        {
            get
            {
                return displayOrderTextField;
            }
            set
            {
                displayOrderTextField = value;
            }
        }

        /// <remarks/>
        public string FareBasisAppText
        {
            get
            {
                return fareBasisAppTextField;
            }
            set
            {
                fareBasisAppTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string FareBasisCode
        {
            get
            {
                return fareBasisCodeField;
            }
            set
            {
                fareBasisCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PriceClassID
        {
            get
            {
                return priceClassIDField;
            }
            set
            {
                priceClassIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement("MarketingInfo")]
        public DescType[] MarketingInfo
        {
            get
            {
                return marketingInfoField;
            }
            set
            {
                marketingInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("RichMedia")]
        public MediaResourceRefType[] RichMedia
        {
            get
            {
                return richMediaField;
            }
            set
            {
                richMediaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SeatCharacteristicCode", DataType = "token")]
        public string[] SeatCharacteristicCode
        {
            get
            {
                return seatCharacteristicCodeField;
            }
            set
            {
                seatCharacteristicCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SeatKeywords")]
        public KeyValuePropertyType[] SeatKeywords
        {
            get
            {
                return seatKeywordsField;
            }
            set
            {
                seatKeywordsField = value;
            }
        }

        /// <remarks/>
        public MeasureType SeatPitchMeasure
        {
            get
            {
                return seatPitchMeasureField;
            }
            set
            {
                seatPitchMeasureField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string SeatProfileID
        {
            get
            {
                return seatProfileIDField;
            }
            set
            {
                seatProfileIDField = value;
            }
        }

        /// <remarks/>
        public MeasureType SeatWidthMeasure
        {
            get
            {
                return seatWidthMeasureField;
            }
            set
            {
                seatWidthMeasureField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement("AirlineTaxonomy")]
        public AirlineTaxonomyType[] AirlineTaxonomy
        {
            get
            {
                return airlineTaxonomyField;
            }
            set
            {
                airlineTaxonomyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("BilateralTimeLimit")]
        public BilateralTimeLimitType[] BilateralTimeLimit
        {
            get
            {
                return bilateralTimeLimitField;
            }
            set
            {
                bilateralTimeLimitField = value;
            }
        }

        /// <remarks/>
        public BookingInstructionsType BookingInstructions
        {
            get
            {
                return bookingInstructionsField;
            }
            set
            {
                bookingInstructionsField = value;
            }
        }

        /// <remarks/>
        public System.DateTime DepositTimeLimitDateTime
        {
            get
            {
                return depositTimeLimitDateTimeField;
            }
            set
            {
                depositTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool DepositTimeLimitDateTimeSpecified
        {
            get
            {
                return depositTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                depositTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Desc")]
        public DescType[] Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        public System.DateTime NamingTimeLimitDateTime
        {
            get
            {
                return namingTimeLimitDateTimeField;
            }
            set
            {
                namingTimeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool NamingTimeLimitDateTimeSpecified
        {
            get
            {
                return namingTimeLimitDateTimeFieldSpecified;
            }
            set
            {
                namingTimeLimitDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }

        /// <remarks/>
        public string RFIC
        {
            get
            {
                return rFICField;
            }
            set
            {
                rFICField = value;
            }
        }

        /// <remarks/>
        public string RFISC
        {
            get
            {
                return rFISCField;
            }
            set
            {
                rFISCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ServiceCode
        {
            get
            {
                return serviceCodeField;
            }
            set
            {
                serviceCodeField = value;
            }
        }

        /// <remarks/>
        public ServiceDefinitionAssociationType ServiceDefinitionAssociation
        {
            get
            {
                return serviceDefinitionAssociationField;
            }
            set
            {
                serviceDefinitionAssociationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ServiceDefinitionID
        {
            get
            {
                return serviceDefinitionIDField;
            }
            set
            {
                serviceDefinitionIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        /// <remarks/>
        public System.DateTime TimeLimitDateTime
        {
            get
            {
                return timeLimitDateTimeField;
            }
            set
            {
                timeLimitDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool TimeLimitDateTimeSpecified
        {
            get
            {
                return timeLimitDateTimeFieldSpecified;
            }
            set
            {
                timeLimitDateTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return methodTextField;
            }
            set
            {
                methodTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OSI_Text")]
        public string[] OSI_Text
        {
            get
            {
                return oSI_TextField;
            }
            set
            {
                oSI_TextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SpecialService")]
        public SpecialServiceType[] SpecialService
        {
            get
            {
                return specialServiceField;
            }
            set
            {
                specialServiceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string UpgradeMethodCode
        {
            get
            {
                return upgradeMethodCodeField;
            }
            set
            {
                upgradeMethodCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string UpgradeNewClassCode
        {
            get
            {
                return upgradeNewClassCodeField;
            }
            set
            {
                upgradeNewClassCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ServiceDefinitionAssociationType
    {

        private object itemField;

        private ItemChoiceType1 itemElementNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("BaggageAllowanceRef", typeof(BaggageAllowanceRefType))]
        [System.Xml.Serialization.XmlElement("SeatProfileRef", typeof(SeatProfileRefType))]
        [System.Xml.Serialization.XmlElement("ServiceBundle", typeof(ServiceBundleType))]
        [System.Xml.Serialization.XmlChoiceIdentifier("ItemElementName")]
        public object Item
        {
            get
            {
                return itemField;
            }
            set
            {
                itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public ItemChoiceType1 ItemElementName
        {
            get
            {
                return itemElementNameField;
            }
            set
            {
                itemElementNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BaggageAllowanceRefType
    {

        private string[] baggageAllowanceRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("BaggageAllowanceRefID", DataType = "token")]
        public string[] BaggageAllowanceRefID
        {
            get
            {
                return baggageAllowanceRefIDField;
            }
            set
            {
                baggageAllowanceRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SeatProfileRefType
    {

        private string[] seatProfileRefIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("SeatProfileRefID", DataType = "token")]
        public string[] SeatProfileRefID
        {
            get
            {
                return seatProfileRefIDField;
            }
            set
            {
                seatProfileRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return maximumServiceQtyField;
            }
            set
            {
                maximumServiceQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MaximumServiceQtySpecified
        {
            get
            {
                return maximumServiceQtyFieldSpecified;
            }
            set
            {
                maximumServiceQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ServiceDefinitionRefID", DataType = "token")]
        public string[] ServiceDefinitionRefID
        {
            get
            {
                return serviceDefinitionRefIDField;
            }
            set
            {
                serviceDefinitionRefIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes", IncludeInSchema = false)]
    public enum ItemChoiceType1
    {

        /// <remarks/>
        BaggageAllowanceRef,

        /// <remarks/>
        SeatProfileRef,

        /// <remarks/>
        ServiceBundle,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return availPeriodField;
            }
            set
            {
                availPeriodField = value;
            }
        }

        /// <remarks/>
        public DescType Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        public OrderingQtyType OrderingQty
        {
            get
            {
                return orderingQtyField;
            }
            set
            {
                orderingQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TermID
        {
            get
            {
                return termIDField;
            }
            set
            {
                termIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return earliestAvailablePeriodDateTimeField;
            }
            set
            {
                earliestAvailablePeriodDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool EarliestAvailablePeriodDateTimeSpecified
        {
            get
            {
                return earliestAvailablePeriodDateTimeFieldSpecified;
            }
            set
            {
                earliestAvailablePeriodDateTimeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public System.DateTime LatestAvailablePeriodDateTime
        {
            get
            {
                return latestAvailablePeriodDateTimeField;
            }
            set
            {
                latestAvailablePeriodDateTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool LatestAvailablePeriodDateTimeSpecified
        {
            get
            {
                return latestAvailablePeriodDateTimeFieldSpecified;
            }
            set
            {
                latestAvailablePeriodDateTimeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
                return maximumQtyField;
            }
            set
            {
                maximumQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MaximumQtySpecified
        {
            get
            {
                return maximumQtyFieldSpecified;
            }
            set
            {
                maximumQtyFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal MinimumQty
        {
            get
            {
                return minimumQtyField;
            }
            set
            {
                minimumQtyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MinimumQtySpecified
        {
            get
            {
                return minimumQtyFieldSpecified;
            }
            set
            {
                minimumQtyFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MetadataType
    {

        private CodesetMetadataType[] codesetMetadataField;

        private FieldMetadataType[] fieldMetadataField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CodesetMetadata")]
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
        [System.Xml.Serialization.XmlElement("FieldMetadata")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CodesetMetadataType
    {

        private string codesetNameField;

        private decimal codesetVersionNumberField;

        private bool codesetVersionNumberFieldSpecified;

        private LangUsageType langUsageField;

        private string ownerIDField;

        private string ownerNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentFunctionsOfferType
    {

        private OfferAssociationType[] offerAssociationField;

        private PaymentSupportedMethodType paymentSupportedMethodField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OfferAssociation")]
        public OfferAssociationType[] OfferAssociation
        {
            get
            {
                return offerAssociationField;
            }
            set
            {
                offerAssociationField = value;
            }
        }

        /// <remarks/>
        public PaymentSupportedMethodType PaymentSupportedMethod
        {
            get
            {
                return paymentSupportedMethodField;
            }
            set
            {
                paymentSupportedMethodField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class OfferAssociationType
    {

        private string[] offerItemRefIDField;

        private string offerRefIDField;

        private string ownerCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("OfferItemRefID", DataType = "token")]
        public string[] OfferItemRefID
        {
            get
            {
                return offerItemRefIDField;
            }
            set
            {
                offerItemRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OfferRefID
        {
            get
            {
                return offerRefIDField;
            }
            set
            {
                offerRefIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerCode
        {
            get
            {
                return ownerCodeField;
            }
            set
            {
                ownerCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentSupportedMethodType
    {

        private ClearanceMethodsType[] clearanceMethodsField;

        private CryptographyKeyType cryptographyKeyField;

        private string paymentBrandCodeField;

        private PaymentMethodAddlInfoType paymentMethodAddlInfoField;

        private string paymentMethodCountryCodeField;

        private bool paymentRedirectionIndField;

        private bool paymentRedirectionIndFieldSpecified;

        private PaymentSurchargeType paymentSurchargeField;

        private string paymentTypeCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ClearanceMethods")]
        public ClearanceMethodsType[] ClearanceMethods
        {
            get
            {
                return clearanceMethodsField;
            }
            set
            {
                clearanceMethodsField = value;
            }
        }

        /// <remarks/>
        public CryptographyKeyType CryptographyKey
        {
            get
            {
                return cryptographyKeyField;
            }
            set
            {
                cryptographyKeyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaymentBrandCode
        {
            get
            {
                return paymentBrandCodeField;
            }
            set
            {
                paymentBrandCodeField = value;
            }
        }

        /// <remarks/>
        public PaymentMethodAddlInfoType PaymentMethodAddlInfo
        {
            get
            {
                return paymentMethodAddlInfoField;
            }
            set
            {
                paymentMethodAddlInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaymentMethodCountryCode
        {
            get
            {
                return paymentMethodCountryCodeField;
            }
            set
            {
                paymentMethodCountryCodeField = value;
            }
        }

        /// <remarks/>
        public bool PaymentRedirectionInd
        {
            get
            {
                return paymentRedirectionIndField;
            }
            set
            {
                paymentRedirectionIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PaymentRedirectionIndSpecified
        {
            get
            {
                return paymentRedirectionIndFieldSpecified;
            }
            set
            {
                paymentRedirectionIndFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PaymentSurchargeType PaymentSurcharge
        {
            get
            {
                return paymentSurchargeField;
            }
            set
            {
                paymentSurchargeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string PaymentTypeCode
        {
            get
            {
                return paymentTypeCodeField;
            }
            set
            {
                paymentTypeCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ClearanceMethodsType
    {

        private CommissionType commissionField;

        private AmountType netClearanceAmountField;

        private string processRuleCodeField;

        private System.DateTime remittanceDateField;

        private bool remittanceDateFieldSpecified;

        /// <remarks/>
        public CommissionType Commission
        {
            get
            {
                return commissionField;
            }
            set
            {
                commissionField = value;
            }
        }

        /// <remarks/>
        public AmountType NetClearanceAmount
        {
            get
            {
                return netClearanceAmountField;
            }
            set
            {
                netClearanceAmountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ProcessRuleCode
        {
            get
            {
                return processRuleCodeField;
            }
            set
            {
                processRuleCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "date")]
        public System.DateTime RemittanceDate
        {
            get
            {
                return remittanceDateField;
            }
            set
            {
                remittanceDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RemittanceDateSpecified
        {
            get
            {
                return remittanceDateFieldSpecified;
            }
            set
            {
                remittanceDateFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class CryptographyKeyType
    {

        private string keyNameTextField;

        private KeyValueType1 keyValueField;

        /// <remarks/>
        public string KeyNameText
        {
            get
            {
                return keyNameTextField;
            }
            set
            {
                keyNameTextField = value;
            }
        }

        /// <remarks/>
        public KeyValueType1 KeyValue
        {
            get
            {
                return keyValueField;
            }
            set
            {
                keyValueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(TypeName = "KeyValueType", Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class KeyValueType1
    {

        private RSA_KeyValueType rSA_KeyValueField;

        /// <remarks/>
        public RSA_KeyValueType RSA_KeyValue
        {
            get
            {
                return rSA_KeyValueField;
            }
            set
            {
                rSA_KeyValueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class RSA_KeyValueType
    {

        private BinaryObjectType exponentField;

        private BinaryObjectType modulusField;

        /// <remarks/>
        public BinaryObjectType Exponent
        {
            get
            {
                return exponentField;
            }
            set
            {
                exponentField = value;
            }
        }

        /// <remarks/>
        public BinaryObjectType Modulus
        {
            get
            {
                return modulusField;
            }
            set
            {
                modulusField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class BinaryObjectType
    {

        private string characterSetCodeField;

        private string encodingCodeField;

        private string fileNameField;

        private string formatField;

        private string mIME_CodeField;

        private string uniformResourceIDField;

        private byte[] valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string CharacterSetCode
        {
            get
            {
                return characterSetCodeField;
            }
            set
            {
                characterSetCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string EncodingCode
        {
            get
            {
                return encodingCodeField;
            }
            set
            {
                encodingCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string FileName
        {
            get
            {
                return fileNameField;
            }
            set
            {
                fileNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public string Format
        {
            get
            {
                return formatField;
            }
            set
            {
                formatField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "token")]
        public string MIME_Code
        {
            get
            {
                return mIME_CodeField;
            }
            set
            {
                mIME_CodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute(DataType = "anyURI")]
        public string UniformResourceID
        {
            get
            {
                return uniformResourceIDField;
            }
            set
            {
                uniformResourceIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlText(DataType = "base64Binary")]
        public byte[] Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentMethodAddlInfoType
    {

        private PaymentCardAddlInfoType paymentCardAddlInfoField;

        private RemarkType[] paymentOtherMethodAddlInfoField;

        /// <remarks/>
        public PaymentCardAddlInfoType PaymentCardAddlInfo
        {
            get
            {
                return paymentCardAddlInfoField;
            }
            set
            {
                paymentCardAddlInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItem("Remark", IsNullable = false)]
        public RemarkType[] PaymentOtherMethodAddlInfo
        {
            get
            {
                return paymentOtherMethodAddlInfoField;
            }
            set
            {
                paymentOtherMethodAddlInfoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentCardAddlInfoType
    {

        private string cardProductTypeCodeField;

        private string cardTypeTextField;

        private SecurePaymentAuthenticationVersionType securePaymentAuthenticationVersionField;

        private bool verificationIndField;

        private bool verificationIndFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string CardProductTypeCode
        {
            get
            {
                return cardProductTypeCodeField;
            }
            set
            {
                cardProductTypeCodeField = value;
            }
        }

        /// <remarks/>
        public string CardTypeText
        {
            get
            {
                return cardTypeTextField;
            }
            set
            {
                cardTypeTextField = value;
            }
        }

        /// <remarks/>
        public SecurePaymentAuthenticationVersionType SecurePaymentAuthenticationVersion
        {
            get
            {
                return securePaymentAuthenticationVersionField;
            }
            set
            {
                securePaymentAuthenticationVersionField = value;
            }
        }

        /// <remarks/>
        public bool VerificationInd
        {
            get
            {
                return verificationIndField;
            }
            set
            {
                verificationIndField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool VerificationIndSpecified
        {
            get
            {
                return verificationIndFieldSpecified;
            }
            set
            {
                verificationIndFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class SecurePaymentAuthenticationVersionType
    {

        private string[] cardEnrollmentVersionTextField;

        private string[] supportedVersionTextField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CardEnrollmentVersionText")]
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
        [System.Xml.Serialization.XmlElement("SupportedVersionText")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaymentSurchargeType
    {

        private AmountType amountRangeMaximumAmountField;

        private AmountType amountRangeMinimumAmountField;

        private decimal percentageRangeMaximumPercentField;

        private bool percentageRangeMaximumPercentFieldSpecified;

        private decimal percentageRangeMinimumPercentField;

        private bool percentageRangeMinimumPercentFieldSpecified;

        private AmountType preciseAmountField;

        private decimal precisePercentField;

        private bool precisePercentFieldSpecified;

        private decimal roundingNumberOfDecimalsNumberField;

        private bool roundingNumberOfDecimalsNumberFieldSpecified;

        private RoundingPrecisionCodeType roundingPrecisionCodeField;

        private bool roundingPrecisionCodeFieldSpecified;

        /// <remarks/>
        public AmountType AmountRangeMaximumAmount
        {
            get
            {
                return amountRangeMaximumAmountField;
            }
            set
            {
                amountRangeMaximumAmountField = value;
            }
        }

        /// <remarks/>
        public AmountType AmountRangeMinimumAmount
        {
            get
            {
                return amountRangeMinimumAmountField;
            }
            set
            {
                amountRangeMinimumAmountField = value;
            }
        }

        /// <remarks/>
        public decimal PercentageRangeMaximumPercent
        {
            get
            {
                return percentageRangeMaximumPercentField;
            }
            set
            {
                percentageRangeMaximumPercentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PercentageRangeMaximumPercentSpecified
        {
            get
            {
                return percentageRangeMaximumPercentFieldSpecified;
            }
            set
            {
                percentageRangeMaximumPercentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal PercentageRangeMinimumPercent
        {
            get
            {
                return percentageRangeMinimumPercentField;
            }
            set
            {
                percentageRangeMinimumPercentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PercentageRangeMinimumPercentSpecified
        {
            get
            {
                return percentageRangeMinimumPercentFieldSpecified;
            }
            set
            {
                percentageRangeMinimumPercentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public AmountType PreciseAmount
        {
            get
            {
                return preciseAmountField;
            }
            set
            {
                preciseAmountField = value;
            }
        }

        /// <remarks/>
        public decimal PrecisePercent
        {
            get
            {
                return precisePercentField;
            }
            set
            {
                precisePercentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool PrecisePercentSpecified
        {
            get
            {
                return precisePercentFieldSpecified;
            }
            set
            {
                precisePercentFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal RoundingNumberOfDecimalsNumber
        {
            get
            {
                return roundingNumberOfDecimalsNumberField;
            }
            set
            {
                roundingNumberOfDecimalsNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RoundingNumberOfDecimalsNumberSpecified
        {
            get
            {
                return roundingNumberOfDecimalsNumberFieldSpecified;
            }
            set
            {
                roundingNumberOfDecimalsNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public RoundingPrecisionCodeType RoundingPrecisionCode
        {
            get
            {
                return roundingPrecisionCodeField;
            }
            set
            {
                roundingPrecisionCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool RoundingPrecisionCodeSpecified
        {
            get
            {
                return roundingPrecisionCodeFieldSpecified;
            }
            set
            {
                roundingPrecisionCodeFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum RoundingPrecisionCodeType
    {

        /// <remarks/>
        Down,

        /// <remarks/>
        Up,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class ResponseParametersResponseType
    {

        private BDC_Type bDCField;

        private CurParameterType[] curParameterField;

        private DeviceType deviceField;

        private LangUsageType[] langUsageField;

        private MarketingMessageType[] marketingMessageField;

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
                return bDCField;
            }
            set
            {
                bDCField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CurParameter")]
        public CurParameterType[] CurParameter
        {
            get
            {
                return curParameterField;
            }
            set
            {
                curParameterField = value;
            }
        }

        /// <remarks/>
        public DeviceType Device
        {
            get
            {
                return deviceField;
            }
            set
            {
                deviceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("LangUsage")]
        public LangUsageType[] LangUsage
        {
            get
            {
                return langUsageField;
            }
            set
            {
                langUsageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("MarketingMessage")]
        public MarketingMessageType[] MarketingMessage
        {
            get
            {
                return marketingMessageField;
            }
            set
            {
                marketingMessageField = value;
            }
        }

        /// <remarks/>
        public MeasurementSystemCodeType MeasurementSystemCode
        {
            get
            {
                return measurementSystemCodeField;
            }
            set
            {
                measurementSystemCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnore()]
        public bool MeasurementSystemCodeSpecified
        {
            get
            {
                return measurementSystemCodeFieldSpecified;
            }
            set
            {
                measurementSystemCodeFieldSpecified = value;
            }
        }

        /// <remarks/>
        public PaxGroupType PaxGroup
        {
            get
            {
                return paxGroupField;
            }
            set
            {
                paxGroupField = value;
            }
        }

        /// <remarks/>
        public PricingParameterType PricingParameter
        {
            get
            {
                return pricingParameterField;
            }
            set
            {
                pricingParameterField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string ReasonCode
        {
            get
            {
                return reasonCodeField;
            }
            set
            {
                reasonCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("Remark")]
        public RemarkType[] Remark
        {
            get
            {
                return remarkField;
            }
            set
            {
                remarkField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum OwnershipTypeCodeType
    {

        /// <remarks/>
        Customer,

        /// <remarks/>
        Seller,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum PresenceTypeCodeType
    {

        /// <remarks/>
        CN,

        /// <remarks/>
        CP,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class MarketingMessageType
    {

        private DescType descField;

        private GeneralAssociationType[] generalAssociationField;

        /// <remarks/>
        public DescType Desc
        {
            get
            {
                return descField;
            }
            set
            {
                descField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("GeneralAssociation")]
        public GeneralAssociationType[] GeneralAssociation
        {
            get
            {
                return generalAssociationField;
            }
            set
            {
                generalAssociationField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class GeneralAssociationType
    {

        private string associatedObjectIDField;

        private string associatedObjectNameField;

        private string associatedObjectPathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AssociatedObjectID
        {
            get
            {
                return associatedObjectIDField;
            }
            set
            {
                associatedObjectIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AssociatedObjectName
        {
            get
            {
                return associatedObjectNameField;
            }
            set
            {
                associatedObjectNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string AssociatedObjectPath
        {
            get
            {
                return associatedObjectPathField;
            }
            set
            {
                associatedObjectPathField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public enum MeasurementSystemCodeType
    {

        /// <remarks/>
        Imperial,

        /// <remarks/>
        Metric,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class PaxGroupType
    {

        private ContactInfoType[] contactInfoField;

        private decimal intendedPaxQtyField;

        private bool intendedPaxQtyFieldSpecified;

        private string paxGroupIDField;

        private string paxGroupNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ContactInfo")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement("TaxExemption")]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class WarningType
    {

        private string codeField;

        private string descTextField;

        private string langCodeField;

        private string ownerNameField;

        private string statusTextField;

        private string tagTextField;

        private string typeCodeField;

        private string uRLField;

        private string warningIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string Code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
            }
        }

        /// <remarks/>
        public string DescText
        {
            get
            {
                return descTextField;
            }
            set
            {
                descTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string LangCode
        {
            get
            {
                return langCodeField;
            }
            set
            {
                langCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string OwnerName
        {
            get
            {
                return ownerNameField;
            }
            set
            {
                ownerNameField = value;
            }
        }

        /// <remarks/>
        public string StatusText
        {
            get
            {
                return statusTextField;
            }
            set
            {
                statusTextField = value;
            }
        }

        /// <remarks/>
        public string TagText
        {
            get
            {
                return tagTextField;
            }
            set
            {
                tagTextField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string TypeCode
        {
            get
            {
                return typeCodeField;
            }
            set
            {
                typeCodeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "anyURI")]
        public string URL
        {
            get
            {
                return uRLField;
            }
            set
            {
                uRLField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(DataType = "token")]
        public string WarningID
        {
            get
            {
                return warningIDField;
            }
            set
            {
                warningIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
    public partial class AugmentationPointType
    {

        private System.Xml.XmlElement[] anyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElement()]
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
    [System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(Namespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlIgnore()]
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
        [System.Xml.Serialization.XmlElement(DataType = "token")]
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
        [System.Xml.Serialization.XmlIgnore()]
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
}
