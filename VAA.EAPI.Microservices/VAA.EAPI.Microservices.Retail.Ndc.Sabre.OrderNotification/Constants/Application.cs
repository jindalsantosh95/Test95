namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Constants
{
    public class Application
    {
        public static readonly string HourText = "H";
        public static readonly string MinuteText = "M";
        public static readonly string SecondsText = "S";
        public static readonly string CouponStatusCodeOpen = "OPE";
        public static readonly string CouponStatusCodeFlownorused = "B";
        public static readonly string CouponStatusCodeVoid = "V";
        public static readonly string CouponStatusCodeCheckedIn = "CK";
        public static readonly string[] CouponStatusReadyToDeliver = { "CKIN", "BOARDED" };
        public const string FlightStatusCodeRemoved = "HX";
        public static readonly string[] FlightStatusCode = { "HK", "KK", "TK", "LK" };
        public static readonly string[] FlightServiceWLStatusCode = { "WL", "HL", "DL", "ML" };
        public static readonly string[] FlightRequestedStatusCode = { "NN", "HL", "WL" };
        public const string CouponStatusVoid = "VOID";
        public const string CouponStatusOpen = "OPEN";
        public const string CouponStatusFlwn = "USED";
        public const string CouponStatusRefunded = "EREF";
        public static string DocTypePaidSeats = "J";
        public static readonly string TicketTypeT = "T";
        public static readonly string TicketTypeEnumChar = "Enum";
        public static readonly string PT = "PT";
        public const string FlightStatusCodeCancelled = "UN";
        public static readonly string ProactiveTtlExpiry = "ProactiveTtlExpiry";
        public static readonly string ReactiveTtlExpiry = "ReactiveTtlExpiry";
        public static readonly string OrderDocumentStatus = "CANCELLED";
        public const string ReasonCodeFlightCancellation = "116";
        public const string ReasonCodeDocumentIssuance = "112";
        public const string ReasonCodeDefault = "100";
        public const string ReasonCodeCancelled = "112";
        public static readonly string DocumentRfdStatus = "RF";
        public static readonly string DocumentOpeStatus = "OPE";
        public static readonly string DocumentVoidStatus = "V";
        public const string ReasonCodeDocumentExchanged = "112";
        public const string ChangeTypeCodeDuplicatePnr = "21";
        public const string ReasonCodeDuplicatePnr = "116";
        public const string DuplicatePnrWarningText = "Duplicate booking identified. Please take appropriate action";
    }
}
