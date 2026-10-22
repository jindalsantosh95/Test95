using System.Security.Cryptography.X509Certificates;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Constants
{
    public class Application
    {
        public static readonly string UpdateOrderEventSubject = "UpdateOrder";
        public static readonly string DateTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string DateFormat = "ddMMMyy";
        public static readonly string DurationFormat = @"dd\:hh\:mm\:ss";
        public static readonly string DateOnlyFormat = "yyyy-MM-dd";
        public static readonly string TimeOnlyFormat = "HH:mm:ss";
        public static readonly string HeaderParameterlocation = "location";
        public static readonly string OrderDocType = "Indirect";
        public static readonly string OwnerAirline = "VS";
        public static readonly string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static readonly string ExternalSystem = "1S";
        public static readonly string JourneyIdPrefix = "JOUR";
        public static readonly string SegmentIdPrefix = "SEG";
        public static readonly string LegIdPrefix = "LEG";
        public static readonly string ServiceIdPrefix = "SV";
        public static readonly string OrderInstructionIdPrefix = "OI";
        public static readonly string PriceClassIdPrefix = "PC";
        public static readonly string OriginDestinationIdPrefix = "OD";
        public static readonly string AdultCustomerID = "ADULT_";
        public static readonly string YoungAdultCustomerID = "YOUNG_";
        public static readonly string ChildCustomerID = "CHILD_";
        public static readonly string InfantCustomerID = "INFANT_";
        public static readonly string OrderStatusConfirmed = "Confirmed";
        public static readonly string OrderStatusCancelled = "Cancelled";
        public static readonly string OrderItemStatusCurrent = "Current";
        public static readonly string TargetCityCode = "09UI";
        public static readonly string DocumentTypeTKT = "TKT";
        public static readonly string DocumentTypeEMD = "EMD";
        public static readonly string SeatStatus = "KK";
        public static readonly string TktInActive = "INACTIVE";
        public static readonly string FreeSeatAmount = "0.00";
        public const string AdultPTCCode = "ADT";
        public const string YoungAdultPTCCode = "GBE";        
        public static readonly string YoungAdultPTCCodeYTH = "YTH";
        public const string ChildPTCCodeCNN = "CNN";
        public const string ChildPTCCodeCHD = "CHD";
        public const string InfantPTCCode = "INF";
        public const string PassportDocumentType = "P";
        public const string VisaDocumentType = "V";
        public const string GreenCardDocumentType = "C";
        public const string NDCPassportDocumentType = "PT";
        public const string NDCVisaDocumentType = "VI";
        public const string NDCGreenCardDocumentType = "GC";
        public const string RedressDocumentType = "AEA";
        public const string DocumentType = "CR";
        public static readonly string GstStatus = "HK";
        public static readonly string GSTA = "GSTA";
        public static readonly string GSTN = "GSTN";
        public static readonly string GSTP = "GSTP";
        public static readonly string GSTE = "GSTE";


        public static string KeyVaultUri = "KeyVaultUri";
        public static string KeyVaultTenantID = "KeyVaultTenantId";
        public static string KeyVaultClientID = "KeyVaultClientId";
        public static string KeyVaultClientSecret = "KeyVaultClientSecret";
        public static readonly string DeltaCertificateName = "DeltaCertificateName";
        public static X509Certificate2 DlCertificate;
        public static readonly string ARNKSegmentType = "4";
        public static readonly string StatusCodePartitionKey = "StatusCode";
        public static readonly string ScheduleChangeStatusCode = "SC";
        public static readonly string Dot = ".";
        public static readonly string Colon = ":";
        public static readonly string ZeroIndicator = "00";
        public static readonly string HourDesignator = "H";
        public static readonly string MinutesDesignator = "M";
        public static readonly string SecondsDesignator = "S";
        public const string DurationHMFormat = "HHMM";
        public const string DurationPTFormat = "PTHHMMSS";
        public static readonly string PTFormat = "PT";
        public static readonly string VSFlightCode = "VS";
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static readonly string TimeLimitDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm'Z'";
        public static readonly string DefaultDeltaTime = "0000A";
        public static readonly string HookedTicketStatus = "HOOKED";

        #region Sabre Variables
        public static readonly string GetReservationAction = "GetReservationRQ";
        public static readonly string GetReservationRQVersion = "1.19.22";
        public static readonly string GetElectronicDocumentAction = "TKT_ElectronicDocumentServicesRQ";
        public static readonly string SabreRequestType = "Stateful";
        public static readonly string SabreViewName = "Full";
        public static readonly string[] SabreSubjectAreas = ["ACCOUNTING_LINE", "ACTIVE", "PASSENGER_DETAILS", "ADDRESS", "ADD_SSR_RPH", "AIR_CABIN", "AFAX", "ANCILLARY", "BAS_EXTENSION", "BSGPROCESSING", "CARRIER_INFORMATION_CALCULATE", "CARRIER_INFORMATION_COMPACT", "CORPORATE_ID", "CUST_INSIGHT_PROFILE", "DK_NUMBER", "DSS", "EXT_FQTV", "FARETYPE", "FQTV", "GFAX", "HEADER", "ITINERARY", "NAME", "PASSENGERDETAILS", "PAY_INFO", "PHONE", "POPULATE_IS_PAST", "PRICE_QUOTE", "PRERESERVEDSEAT", "PRICING_INFORMATION", "RECEIVED", "RECORD_LOCATOR", "REMARKS", "REMARK_ACTION_CODE", "TICKETING", "VCR", "EXTENDED_PRS_DATA", "EXTENDED_GENDER", "EXTENDED_INFANT_GENDER", "OPENRESDATA"];
        #endregion
    }
}
