namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Constants
{
    public class Application
    {
        public static readonly string EventSpecversion = "1.0";
        public static readonly string OrderChangeEventSubject = "OrderChange";
        public static readonly string OrderCancelEventSubject = "OrderCancel";
        public static readonly string HeaderParameterlocation = "location";
        public static readonly string OrderDocType = "Indirect";
        public static readonly string OwnerAirline = "VS";
        public static readonly string DateFormat = "ddMMMyy";
        public static readonly string DateOnlyFormat = "yyyy-MM-dd";
        public static readonly string TimeOnlyFormat = "HH:mm:ss";
        public const string DeleteOrderMgmtEventStoreHandlerTopicName = "deleteorderchangeeventstoremessage";
        public const string DeleteOrdermgmtEventStoreSubscriptionName = "DeleteOrderChangeEventStoreMessageSubscriber";
        public static readonly string OrderChangeEventType = "VAA.Order.InitiateOrderChange";
        public static readonly string DateTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string InputDateTimeFormat = "yyyy-MM-ddTHH:mm";
        public const string OrderChangeEventHandlerTopicName = "ndcorderchangerequesteventmessage";
        public const string OrderChangeEventSubscriptionName = "OrderChangeEventSubscriber";
        public static readonly int PNRLength = 6;
        public static readonly int PtcLength = 3;
        public static readonly int IATAOriginAirportCodeLength = 3;
        public static readonly int IATADestAirportCodeLength = 3;
        public static readonly int SegmentIdLength = 3;
        public static readonly int MarketingCarrierLength = 2;
        public static readonly int ClassOfServiceLength = 1;
        public static readonly int MarketingFlightNumberMinLength = 1;
        public static readonly int MarketingFlightNumberMaxLength = 4;
        public static readonly string EmailRegex = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public const string OrderCancelEventHandlerTopicName = "ndccancelpnreventmessage";
        public const string OrderCancelEventSubscriptionName = "CancelPnrEventMessageSubscriber";
        public static readonly string NDCCancelPnrTopicName = "ndccancelpnreventmessage"; 
        public static readonly string NDCNameUpdateEventHandlerTopicName = "ndcnameupdateeventmessage";
        public static readonly string NDCOrderHistoryUpdateSubscriberTopicName = "ndcupdateorderhistoryeventmessage";
        public static readonly string NDCArcDocumentUpdateSubscriberTopicName = "ndcarcdocumentupdateeventmessage";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string EventDataContentType = "application/json";
        public static readonly string HeaaderParameterlocation = "location";
        public static readonly string ManageCustomerDateFormat = "yyyy-MM-dd";
        public static readonly string DocumentTypeTKT = "TKT";
        public static readonly string DocumentTypeEMD = "EMD";
        public static readonly string UpdateOrderEventSubject = "UpdateOrder";
        
        public const string SplitPnrEventSubject = "SplitPnr";
        public const string SplitPnrEventHandlerTopicName = "ndcorderchangerequesteventmessage";
        public const string SplitPnrEventSubscriptionName = "SplitPnrEventSubscriber";

        public const string ManageCustomerEventHandlerTopicName = "ndcorderchangerequesteventmessage";
        public const string ManageCustomerEventSubscriptionName = "ManageCustomerEventSubscriber";
        public static readonly string ManageCustomerEventSubject = "ManageCustomer";
        public static readonly string ManageCustomerAmendApisEventSubject = "ManageCustomerAmendApis";
        public static readonly string ManageCustomerNameCorrectionEventSubject = "ManageCustomerNameCorrection";
        public static readonly string ManageSpecialServiceEventSubject = "ManageSpecialService";
        public const string InvalidDOBMessage = "INVALID DATE OF BIRTH";
        public const string InvalidGenderMessage = "CK GENDER";
        public static readonly string PassportDocType = "Passport";
        public static readonly string PTDocType = "PT";
        public static readonly string GreenCardDocType = "GreenCard";
        public static readonly string GCDocType = "GC";
        public static readonly string VisaDocType = "Visa";
        public static readonly string VIDocType = "VI";
        public static readonly string FlightService = "FlightService";
        public static readonly string OrderStatusConfirmed = "Confirmed";
        public static readonly string SplitPNRVersion = "2.0.3";
        public static readonly string EndTransactionVersion = "2.1.0";
        public static readonly bool ReturnHostCommandvalue = true;
        public static readonly string JourneyIdPrefix = "JOUR";
        public static readonly string SegmentIdPrefix = "SEG";
        public static readonly string LegIdPrefix = "LEG";
        public static readonly string PriceClassIdPrefix = "PC";
        public static readonly string OriginDestinationIdPrefix = "OD";
        public static readonly string DurationFormat = @"dd\:hh\:mm\:ss";

        #region Sabre Variables
        public const string Status = "NN";
        public const string RequestType = "Stateful";
        public static readonly string SabreSource = "API";
        public static readonly string ReceivedFromName = "NDC";
        public static readonly string SabreVersion = "1.3.0";
        public static readonly string SabreEndTransactionLLSVersion = "1.3.0";
        public static readonly string SabreOTACancelVersion = "2.0.2";
        public static readonly string SabrePassportDocType = "P";
        public static readonly string SabreGreenCardDocType = "C";
        public static readonly string SabreVisaDocType = "V";
        public static readonly string RedressDocType = "AEA";
        public static readonly string KnowntravelerDocType = "CR";
        public static readonly string Infant = "INF";
        public static readonly string DesignatePrinterAction = "DesignatePrinterLLSRQ";
        public static readonly string GetReservationAction = "GetReservationRQ";
        public static readonly string GetReservationRQVersion = "1.19.22";
        public static readonly string[] SabreSubjectAreas = ["ACCOUNTING_LINE", "ACTIVE", "PASSENGER_DETAILS", "ADDRESS", "ADD_SSR_RPH", "AIR_CABIN", "AFAX", "ANCILLARY", "BAS_EXTENSION", "BSGPROCESSING", "CARRIER_INFORMATION_CALCULATE", "CARRIER_INFORMATION_COMPACT", "CORPORATE_ID", "CUST_INSIGHT_PROFILE", "DK_NUMBER", "DSS", "EXT_FQTV", "FARETYPE", "FQTV", "GFAX", "HEADER", "HISTORICAL", "ITINERARY", "NAME", "PASSENGERDETAILS", "PAY_INFO", "PHONE", "POPULATE_IS_PAST", "PRICE_QUOTE", "PRERESERVEDSEAT", "PRICING_INFORMATION", "RECEIVED", "RECORD_LOCATOR", "REMARKS", "REMARK_ACTION_CODE", "TICKETING", "VCR", "EXTENDED_PRS_DATA", "EXTENDED_GENDER", "EXTENDED_INFANT_GENDER", "OPENRESDATA"];
        public static readonly string SabreViewName = "Full";
        public static readonly string OTACancelLLSAction = "OTA_CancelLLSRQ";
        public static readonly string EnhancedAirBookAction = "EnhancedAirBookRQ";
        public static readonly string EnhancedEndTransactionRQVersion = "1.0.0";
        public static readonly string EnhancedEndTransactionAction = "EnhancedEndTransactionRQ";
        public static readonly string SabreCommandLLSAction = "SabreCommandLLSRQ";
        public static readonly string UpdateReservationAction = "UpdateReservationRQ";
        public static readonly string SabreCommandLLSVersion = "2.0.0";
        public static readonly string SabreCommandLLSNumResponses = "1";
        public static readonly string VoidAirTicketAction = "VoidTicketLLSRQ";
        public static readonly string UpdatePNRVersion = "1.1.0";
        public const string WaitIntervalField = "1000";
        public static readonly string EnhancedAirBookRQVersion = "3.10.0";
        public static readonly string EnhancedAirTicketAction = "AirTicketRQ";
        public static readonly string DesignatePrinterProfileNumber = "1";
        public static readonly string DirectTicketingDesignatePrinterProfileNumber = "2";
        public static readonly string CountryCode = "AT";
        public static readonly string PrinterCode = "111574";
        public static readonly string TargetCityCode = "09UI";
        public static readonly string ManualPriceQuoteAction = "ManualPriceQuoteAddInfoLLSRQ";
        public static readonly string GetElectronicDocumentAction = "TKT_ElectronicDocumentServicesRQ";
        public static readonly string SSRCode = "OSI";
        public static readonly string OSIText = "SI VS REFUND DUE TO INVOL VS SCHED CHG";
        public static readonly string OSICorporateCode = "DSCB";

        public static readonly string UpdatePNRAction = "UpdatePassengerNameRecordRQ";
        public static readonly string EndTransactionAction = "EndTransactionLLSRQ";
        public static readonly string SplitPNRAction = "TravelItineraryDivideLLSRQ";
        public static readonly string Child = "CHD";
        public static readonly string CNN = "CNN";
        public static readonly string InfantSSRCode = "INFT";
        public static readonly string ChildSSRCode = "CHLD";
        public static readonly string OrderItemStatusCurrent = "Current";

        public static readonly string ErrorMsg = "Error occurred in";
        public static readonly string BabyMeal = "BBML";
        public static readonly string ChildMeal = "CHML";
        public static readonly string GBESabreCode = "YTH";
        public static readonly string GBECode = "GBE";
        public static readonly string TktInActive = "INACTIVE";
        public static readonly string AirTicketRQAction = "AirTicketRQ";
        public static readonly string PaymentType = "CA";
        #endregion
    }
}
