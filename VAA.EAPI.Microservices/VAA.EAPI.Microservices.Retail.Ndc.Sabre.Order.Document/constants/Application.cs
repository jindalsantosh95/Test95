namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Constants
{
    public static class Application
    {
        public static readonly string NDCUpdateTicketTopicName = "ndcupdateticketeventmessage";
        public static readonly string NDCCancelPnrTopicName = "ndccancelpnreventmessage";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string EventSpecversion = "1.0";
        public static readonly string EventDataContentType = "application/json";
        public static readonly string OrderItemStatusCurrent = "Current";


        #region Sabre Variables
        public static readonly string CreatePNRAction = "CreatePassengerNameRecordRQ";
        public static readonly string ManualPriceQuoteAction = "ManualPriceQuoteAddInfoLLSRQ";
        public static readonly string EnhancedEndTransactionAction = "EnhancedEndTransactionRQ";
        public static readonly string GetReservationAction = "GetReservationRQ";
        public static readonly string TicketCouponRQAction = "eTicketCouponRQ";
        public static readonly string GetReservationRQVersion = "1.19.22";
        public static readonly string EnhancedEndTransactionRQVersion = "1.0.0";
        public static readonly string TicketCouponVersion = "2.0.0";
        public static readonly string ReceivedFrom = "API TEST";
        public static readonly string RecordValue = "1";
        public static readonly string SabreRequestType = "Stateful";
        public static readonly string SabreViewName = "Full";
        public static readonly string[] SabreSubjectAreas = ["ACCOUNTING_LINE", "ACTIVE", "ADDRESS", "ADD_SSR_RPH", "AIR_CABIN", "AFAX", "ANCILLARY", "BAS_EXTENSION", "BSGPROCESSING", "CARRIER_INFORMATION_CALCULATE", "CARRIER_INFORMATION_COMPACT", "CORPORATE_ID", "CUST_INSIGHT_PROFILE", "DK_NUMBER", "DSS", "EXT_FQTV", "FARETYPE", "FQTV", "GFAX", "HEADER", "HISTORICAL", "ITINERARY", "NAME", "PASSENGERDETAILS", "PAY_INFO", "PHONE", "POPULATE_IS_PAST", "PRICE_QUOTE", "PRERESERVEDSEAT", "PRICING_INFORMATION", "RECEIVED", "RECORD_LOCATOR", "REMARKS", "REMARK_ACTION_CODE", "TICKETING", "VCR", "EXTENDED_PRS_DATA"];        
        public static readonly string AirTicketRQAction = "AirTicketRQ";
        public static readonly string OwnerAirline = "VS";
        public static readonly string PaymentType = "CA";
        public static readonly string SabreSource = "API";
        public static readonly string SabreVersion = "1.3.0";
        public static readonly string CountryCode = "AT";
        public static readonly string PrinterCode = "111574";
        public static readonly string TargetCityCode = "09UI";
        public static readonly string GetElectronicDocumentAction = "TKT_ElectronicDocumentServicesRQ";
        public static readonly string DesignatePrinterProfileNumber = "1";

        #endregion
    }
}
