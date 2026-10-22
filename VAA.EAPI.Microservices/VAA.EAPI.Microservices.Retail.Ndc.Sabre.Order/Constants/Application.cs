namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Constants
{
    public class Application
    {
        public static readonly string SpecVersion = "0.1";
        public static readonly string HeaderParameterlocation = "location";
        public static readonly string OrderCreateEventSubject = "OrderCreate";
        public static readonly string OrderCreateEventType = "VAA.Order.InitiateOrderCreate";
        public const string OrderEventHandlerTopicName = "orderrequesteventmessage";
        public const string OrderEventSubscriptionName = "OrderEventSubscriber";
        public static readonly string OrderDocType = "Indirect";
        public static readonly string OwnerAirline = "VS";
        public static readonly string DateTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string DateFormat = "ddMMMyy";
        public const string DeleteOrderEventStoreHandlerTopicName = "deleteordereventstoremessage";
        public const string DeleteOrderEventStoreSubscriptionName = "DeleteOrderEventStoreMessageSubscriber";
        public static readonly string PassportDocType = "Passport";
        public static readonly string PTDocType = "PT";
        public static readonly string GreenCardDocType = "GreenCard";
        public static readonly string GCDocType = "GC";
        public static readonly string VisaDocType = "Visa";
        public static readonly string VIDocType = "VI";
        public static readonly string HK = "HK";
        public static readonly string GSTN = "GSTN";
        public static readonly string GSTA = "GSTA";
        public static readonly string GSTE = "GSTE";
        public static readonly string GSTP = "GSTP";
        public static readonly string SSRGSTPDetails = "SSR GSTP VS HK1/";
        public static readonly string SSRGSTEDetails = "SSR GSTE VS HK1/";
        public static readonly string SSRGSTADetails = "SSR GSTA VS HK1/";
        public static readonly string SSRGSTNDetails = "SSR GSTN VS HK1/";
        public static readonly string RedressDocType = "Redress";
        public static readonly string AEADocType = "AEA";
        public static readonly string CRDocType = "CR";
        public static readonly string SabreSource = "API";
        public static readonly string SabreVersion = "1.3.0";
        public static readonly string Infant = "INF";
        public static readonly string CreatePNRAction = "CreatePassengerNameRecordRQ";
        public static readonly string ManualPriceQuoteAction = "ManualPriceQuoteAddInfoLLSRQ";
        public static readonly string Child = "CHD";
        public static readonly string CNN = "CNN";
        public static readonly string InfantSSRCode = "INFT";
        public static readonly string ChildSSRCode = "CHLD";
        public static readonly string OrderItemStatusCurrent = "Current";
        public static readonly string GetReservationAction = "GetReservationRQ";
        public static readonly string GetReservationRQVersion = "1.19.22";
        public static readonly string[] SabreSubjectAreas = ["ACCOUNTING_LINE", "ACTIVE", "ADDRESS", "ADD_SSR_RPH", "AIR_CABIN", "AFAX", "ANCILLARY", "BAS_EXTENSION", "BSGPROCESSING", "CARRIER_INFORMATION_CALCULATE", "CARRIER_INFORMATION_COMPACT", "CORPORATE_ID", "CUST_INSIGHT_PROFILE", "DK_NUMBER", "DSS", "EXT_FQTV", "FARETYPE", "FQTV", "GFAX", "HEADER", "HISTORICAL", "ITINERARY", "NAME", "PASSENGERDETAILS", "PAY_INFO", "PHONE", "POPULATE_IS_PAST", "PRICE_QUOTE", "PRERESERVEDSEAT", "PRICING_INFORMATION", "RECEIVED", "RECORD_LOCATOR", "REMARKS", "REMARK_ACTION_CODE", "TICKETING", "VCR", "EXTENDED_PRS_DATA"];
        public static readonly string SabreRequestType = "Stateful";
        public static readonly string SabreViewName = "Full";
        public static readonly string FlightService = "FlightService";
        public static readonly string Meal = "Meal";
        public static readonly string Disability = "Disability";
        public static readonly string GBESabreCode = "YTH";
        public static readonly string GBECode = "GBE";
        public static readonly string BabyMeal = "BBML";
        public static readonly string ChildMeal = "CHML";
        public static readonly string CtceSSRCode = "CTCE";
        public static readonly string CtcmSSRCode = "CTCM";
        public static readonly string CtcrSSRCode = "CTCR";
        public static readonly string OsiSSRCode = "OSI";
        public static readonly string OsiCorporateCode = "DSCB";
        public static readonly string DurationFormat = @"dd\:hh\:mm\:ss";
        public static readonly string DateOnlyFormat = "yyyy-MM-dd";
        public static readonly string JourneyIdPrefix = "JOUR";
        public static readonly string SegmentIdPrefix = "SEG";
        public static readonly string LegIdPrefix = "LEG";
        public static readonly string ServiceIdPrefix = "SV";
        public static readonly string OrderInstructionIdPrefix = "OI";
        public static readonly string PriceClassIdPrefix = "PC";
        public static readonly string OriginDestinationIdPrefix = "OD";

        public static readonly string ProactiveTtlExpiryEventSubject = "ProactiveTtlExpiry";
        public static readonly string ReactiveTtlExpiryEventSubject = "ReactiveTtlExpiry";

        public static readonly string PrivateFareTypeCode = "758";
        public static readonly string PrivateFareOsiElement = "OTHS ITWA";
        public static readonly string PrivateFareITX = "ITX";
        public static readonly string PrivateFareITXPreferred = "ITX Preferred";
    }
}
