namespace VAA.EAPI.Microservices.Retail.Order.Management.Constants
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
        public static readonly string RemarksV3EndPoint = "RemarksV3EndPoint";
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
        public static readonly string PNRValidationFeatureFlag = "InflightOrders:PNRValidationFeatureFlag";
        public static readonly string DatabaseID = "InflightOrders:OneTicketDatabaseID";
        public static readonly string OneTicketCommonRefDBConnString = "InflightOrders:OneTicketCommonRefDBConnString";
        public static readonly string CommonCachingSlidingExpirationInDay = "InflightOrders:CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "InflightOrders:CommonCachingAbsoluteExpirationInDay";
        public static readonly string AzureAdB2CExtensionsAppId = "AzureAdB2C:Extensions-AppId";
        public static readonly string GraphB2CAPIVersion = "GraphB2CAPI-version";
        public static readonly string DeleteDocEventType = "DeleteDocEventType";
        public static readonly string ManageCustomerResponseUrl = "ManageCustomerResponseUrl";
        public static readonly string ManageSpecialServiceResponseUrl = "ManageSpecialServiceResponseUrl";

        public static readonly string DeltaPassengerV4EndPoint = "DeltaPassengerV4EndPoint";
        public static readonly string FirstCustomerEmailValidationOnly = "IsEmailvalidationForFirstCustomerOnly";
        public static readonly string GenderList = "GenderList";
        public static readonly string OrderChangeSeatAddServiceBusTopicName = "OrderChangeSeatAddServiceBusTopicName";
        public static readonly string OrderChangeSeatDeleteServiceBusTopicName = "OrderChangeSeatDeleteServiceBusTopicName";
        public static readonly string SeatServiceBusConnectionString = "SeatServiceBusConnectionString";

        #endregion

        #region Offer Search
        public static readonly string OfferSearchClientId = "OfferSearchClientId";
        public static readonly string OfferSearchClientSecret = "OfferSearchClientSecret";
        public static readonly string AzureADEndpoint = "AzureADEndpoint";
        public static readonly string OfferSearchResouceId = "OfferSearchResouceId";
        public static readonly string OfferSearchEndPoint = "OfferSearchEndPoint";
        public static readonly string OfferSearchOCPKey = "OfferSearchOCPKey";
        public static string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static string ChannelId = "ChannelId";
        #endregion

        #region StampFCNumberToPNR
        public static readonly string ChannelIdToAddSSR = "ChannelIdToAddSSR";
        #endregion
    }
}
