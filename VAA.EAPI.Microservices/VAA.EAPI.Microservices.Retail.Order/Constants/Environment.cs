namespace VAA.EAPI.Microservices.Retail.Order.Constants
{
    public class Environment
    {
        #region OrderChange

        public static readonly string OrderChangeResponseUrl = "OrderChangeResponseUrl";
        public static readonly string ApplicationId = "ApplicationId";
        public static readonly string TestLab = "TestLab";
        public static readonly string DebugOptions = "DebugOptions";
        public static readonly string PoolType = "PoolType";
        public static readonly string RoleIndicator = "RoleIndicator";
        public static readonly string SecurityId = "SecurityId";
        public static readonly string ResponseThresholdInSec = "ResponseThresholdInSec";
        public static readonly string ResponseThreadDelayInSec = "ResponseThreadDelayInSec";
        public static readonly string Air4PnrCommonEndPoint = "Air4PnrCommonEndPoint";
        public static readonly string Air4TicketingProV3EndPoint = "Air4TicketingProV3EndPoint";
        public static readonly string SegmentV4EndPoint = "SegmentV4EndPoint";
        public static readonly string SpecialServiceRequestV4EndPoint = "SpecialServiceRequestV4EndPoint";
        public static readonly string SegmentV4InsertReqStatus = "SegmentV4InsertReqStatus";
        public static readonly string SegmentV4InsertReqOpenFlag = "SegmentV4InsertReqOpenFlag";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";
        public static readonly string PtcLength = "PtcLength";
        public static readonly string OriginAirportCodeLength = "OriginAirportCodeLength";
        public static readonly string DestAirportCodeLength = "DestAirportCodeLength";
        public static readonly string SegmentIdLength = "SegmentIdLength";
        public static readonly string MarketingCarrierLength = "MarketingCarrierLength";
        public static readonly string ClassOfServiceLength = "ClassOfServiceLength";
        public static readonly string MarketingFlightNumberMinLength = "MarketingFlightNumberMinLength";
        public static readonly string MarketingFlightNumberMaxLength = "MarketingFlightNumberMaxLength";
        public static readonly string PrinterData = "PrinterData";
        public static readonly string ReferenceDataMicroServiceDBKey = "MicroServiceDBKey";
        public static readonly string ReferenceDataTableName = "ReferenceDataTableName";
        public static string ServiceBusConnectionString = "ServiceBusConnectionString";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";
        public static readonly string EnableMetaPartnerEvent = "EnableMetaPartnerEvent";


        #endregion

        #region Inflight Orders

        public static readonly string PNRValidationFeatureFlag = "InflightOrders:PNRValidationFeatureFlag";
        public static readonly string DatabaseID = "InflightOrders:OneTicketDatabaseID";
        public static readonly string OneTicketCommonRefDBConnString = "InflightOrders:OneTicketCommonRefDBConnString";
        public static readonly string CommonCachingSlidingExpirationInDay = "InflightOrders:CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "InflightOrders:CommonCachingAbsoluteExpirationInDay";
        public const string CosmosSCVDatabaseEndPoint = "InflightOrders:SCVCosmos:DatabaseEndpoint";
        public const string CosmosSCVDatabaseName = "InflightOrders:SCVCosmos:DatabaseName";
        public const string WorkingProfileCollection = "InflightOrders:SCVCosmos:WorkingProfileCollection";
        public const string CustomerProfileCollection = "InflightOrders:SCVCosmos:CustomerProfileCollection";
        public const string FlightCollectionCollection = "InflightOrders:SCVCosmos:FlightCollectionCollection";
        public const string OrderAssociationResponseUrl = "InflightOrders:OrderAssociationResponseUrl";
        public const string OrdersCollectionName = "InflightOrders:SCVCosmos:OrdersCollectionName";
        public static readonly string AzureAdB2CExtensionsAppId = "AzureAdB2C:Extensions-AppId";
        public static readonly string GraphB2CAPIVersion = "GraphB2CAPI-version";

        #endregion

        #region OrderCreate

        public static readonly string DlOfferAuthenticationUrl = "DlOfferAuthenticationUrl";
        public static readonly string DlApiApplicationId = "DlApiApplicationId";
        public static readonly string DlApiChannelId = "DlApiChannelId";
        public static readonly string DlOrderApiUri = "DlOrderApiUri";
        public static readonly string DlOrderClientId = "DlOrderClientId";
        public static readonly string DlOrderClientSecret = "DlOrderClientSecret";
        public static readonly string OrderCreateResponseUrl = "OrderCreateResponseUrl";
        
        public static readonly string CustomerOrderEnrollmentServiceBusTopicName = "CustomerOrderEnrollmentServiceBusTopicName";
        public static readonly string CustomerOrderEnrollmentServiceBusConnectionString = "CustomerOrderEnrollmentServiceBusConnectionString";

        public static readonly string DlGooglePay = "DlGooglePay";
        public static readonly string GooglePayCode = "GooglePayCode";

        public static readonly string DlAwsOrderAuthenticationUrl = "DlAwsOrderAuthenticationUrl";
        public static readonly string DlAwsOrderClientId = "DlAwsOrderClientId";
        public static readonly string DlAwsOrderClientSecret = "DlAwsOrderClientSecret";
        public static readonly string DlAwsOrderUri = "DlAwsOrderUri";
        public static readonly string SwitchToAws = "SwitchToAws";
        public static readonly string DlAwsOrderLabId = "DlAwsOrderLabId";
        public static readonly string EnableChildMeal = "EnableChildMeal";

        public static readonly string ServiceBusTopicNameForInsurancePurchase = "ServiceBusTopicNameForInsurancePurchase";
        public static readonly string ServiceBusTopicInsuranceDecision = "ServiceBusTopicInsuranceDecision";

        public static readonly string ServiceBusTopicClearPaymentDetails = "ServiceBusTopicClearPaymentDetails";
        public static readonly string EnableClearCVV = "EnableClearCVV";
        public static readonly string DisableBasicEnrolment = "DisableBasicEnrolment";

        #endregion

    }
}
