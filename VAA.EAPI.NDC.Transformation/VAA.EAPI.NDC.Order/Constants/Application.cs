using System;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Constants
{
    public static class Application
    {
        public static readonly string LangCode = "EN";
        public static readonly string RegexToValidateAlphaNumeric = @"^[a-zA-Z0-9]*$";
        public static readonly string RegexToValidateNumeric = @"^\d+$";
        public static readonly string NDC = "NDC";
        public static readonly int PNRLength = 6;
        public static readonly string RegexToValidateEmail = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$";
        public static readonly string EmailMaxLength = "64";
        public static readonly string PT = "PT";
        public static readonly string GC = "GC";
        public static readonly string CR = "CR";
        public static readonly string VI = "VI";
        public static readonly string Sabre_PT = "P";
        public static readonly string Sabre_GC = "C";
        public static readonly string Sabre_VI = "V";
        public static readonly string AEA = "AEA";
        public static readonly string SurnameMaxLength = "35";
        public static readonly string GivenNameMaxLength = "19";
        public static readonly string AgentTypeIATA = "IATA";
        public static readonly string IndirectAgent = "TIDS";
        public static readonly string VSClientId = "932";
        public static readonly string CNN = "CNN";
        public static readonly string CHD = "CHD";
        public static readonly string INF = "INF";
        public static readonly string ValidGender = "M,F,U,X";
        public static readonly string RegexToValidateAlphabets = @"^[a-zA-Z]+$";
        public static readonly string RegexToValidateCountryCode = @"^\+?\d+";
        public const string AdultPTCCode = "ADT";
        public const string InfantPTCCode = "INF";
        public const string ChildPTCCode = "CHD";
        public const string YoungPTCCode = "GBE";
        public const string YouthPTCCode = "YTH";
        public static readonly string FlightOfferServiceErrorCodeGeneral = "G009;";
        public static readonly string FlightOfferServiceErrorCodeNoDataMatch = "10118";
        public static readonly string H = "H";
        public static readonly string M = "M";
        public static readonly string S = "S";
        public static readonly string OwnerAirline = "VS";
        public static readonly string Service = "Amenity";
        public static readonly string CheckedBagService = "Free Checked Bag Allowance";
        public static readonly string CarryOnBagService = "Free Cabin Bag Allowance";
        public static readonly string SRV = "SRV";
        public static readonly string SV = "SV";
        public static readonly string TaxCodeXF = "XF";
        public static readonly string TaxCodeZP = "ZP";
        public static readonly string SSRGSTP = "GSTP";
        public static readonly string SSRGSTE = "GSTE";
        public static readonly string SSRGSTA = "GSTA";
        public static readonly string SSRGSTN = "GSTN";
        public static readonly string SSRGSTPDetails = "SSR GSTP VS HK1/IND/";
        public static readonly string SSRGSTEDetails = "SSR GSTE VS HK1/IND/";
        public static readonly string SSRGSTADetails = "SSR GSTA VS HK1/IND/";
        public static readonly string SSRGSTNDetails = "SSR GSTN VS HK1/IND/";
        public static readonly string passportDocumentType = "Passport";
        public static readonly string visaDocumentType = "Visa";
        public static readonly string knownTravellerDocumentType = "KnownTraveller";
        public static readonly string greenCardDocumentType = "GreenCard";
        public static readonly string RedressDocType = "Redress";
        public static readonly string OrderRetrieveEventSubject = "OrderRetrieve";
        public static readonly string OrderHistoryEventSubject = "OrderHistory";
        public static readonly string OrderChangeEventSubject = "OrderChange";
        public static readonly string OrderListEventSubject = "OrderList";
        public static readonly string OrderChangeNotifEventSubject = "OrderChangeNotif";
        public static readonly string OrderChangeDuplicatePNRNotifEventSubject = "OrderChangeDuplicatePNRNotif";
        public const string InvoluntaryScheduleChangeReasonCode = "110";
        public const string InRuleScheduleMinorScenario = "SCH_MINOR";
        public static readonly string NDCVersion = "NDC 21.3.4";
        public static readonly string CI = "CI";
        public static readonly string SEG = "SEG";
        public static readonly string OD = "OD";
        public static readonly string PEN = "PEN";
        public static readonly string PC = "PC";
        public static readonly string DATAMARKETINGSEGMENT = "DMS";
        public static readonly string DATAMARKETINGREF = "DOS";
        public static readonly string DATEDOPERATINGLEGREF = "DOL";
        public static readonly string SERVICEREF = "SRV";
        public static readonly string BAGA = "BAGA";
        public static readonly string JOUR = "JOUR";
        public static readonly string Meal = "Meal";
        public static readonly string Disability = "Disability";
        public static readonly string TicketTypeEnumChar = "Enum";
        public const string OrderItemStatusCodeActive = "ACTIVE";
        public const string OrderItemStatusCodeOpened = "OPENED";
        public const string OrderItemStatusCodeClosed = "CLOSED";
        public const string OrderItemStatusCodeCancel = "CANCELLED";
        public const string DeliveryStatusCodeRequested = "REQUESTED";
        public const string DeliveryStatusCodeConfirmed = "CONFIRMED";
        public const string DeliveryStatusCodeReadyToProceed = "READY TO PROCEED";
        public const string DeliveryStatusCodeReadyToDeliver = "READY TO DELIVER";
        public const string DeliveryStatusCodeDelivered = "DELIVERED";
        public const string DeliveryStatusCodeRemoved = "REMOVED";
        public static readonly string NumberOfBooklets = "1";
        public static string DocTypePaidSeats = "J";
        public static readonly string TicketTypeT = "T";
        public const string SearchOpenCouponStatus = "OPEN";
        public const string SearchFlownCouponStatus = "FLWN";
        public const string SearchUsedCouponStatus = "USED";
        public const string SearchVoidCouponStatus = "VOID";
        public const string SearchRefundCouponStatus = "RFND";
        public const string SearchCheckedInCouponStatus = "CKIN";
        public const string SearchEMDOpenCouponStatus = "O";
        public static readonly string CouponStatusCodeOpen = "OPE";
        public static readonly string CouponStatusCodeFlownorused = "B";
        public static readonly string CouponStatusCodeVoid = "V";
        public static readonly string CouponStatusCodeCheckedIn = "CK";
        public static readonly string CouponStatusCodeRefund = "RF";
        public static string AllFlownCouponStatus = "FLOWN/USED";
        public const string PAX = "PAX";
        public static string AdultCustomerID = "ADULT_";
        public static string YoungAdultCustomerID = "YOUNG_";
        public static string ChildCustomerID = "CHILD_";
        public static string InfantCustomerID = "INFANT_";
        public static string PassengerCustomerID = "PASSENGER_";
        public const string ServiceStatusCodeCancelled = "CANCELLED";
        public const string ServiceStatusCodeTransferred = "TRANSFERRED";
        public const string ServiceStatusCodeWaitlisted = "WAITLISTED";
        public static readonly string[] CouponStatusReadyToDeliver = { "CKIN", "BOARDED" };
        public const string FlightStatusCodeRemoved = "HX";
        public const string FlightStatusCodeCancelled = "UN";
        public const string FlightStatusCodeTransferred = "TR";
        public static readonly string[] FlightStatusCode = { "HK", "KK", "TK", "LK" };
        public static readonly string[] FlightServiceWLStatusCode = { "WL", "HL", "DL", "ML" };
        public static readonly string[] FlightRequestedStatusCode = { "NN", "HL", "WL" };
        public const string CouponStatusVoid = "VOID";
        public const string CouponStatusOpen = "OPEN";
        public const string CouponStatusFlwn = "USED";
        public const string CouponStatusRefunded = "EREF";
        public const string FlownEMDCouponStatus = "F";
        public static readonly string SeatRficCode = "A";
        public static readonly string SeatRfiscCode = "0B5";
        public static readonly string DefaultServiceID = "1";
        public static string ChangePenaltyType = "Change";
        public static string CancellationPenaltyType = "Cancel";
        public static readonly string BAG = "BAG";
        public static readonly string CAB = "CAB";
        public static readonly string SSR = "SSR";
        public static readonly string CheckedBaggage = "Checked";
        public static readonly string CarryOnBaggage = "CarryOn";
        public static readonly string CheckedBagWidthCmt = "43";
        public static readonly string CheckedBagWidthInh = "16";
        public static readonly string CheckedBagHeightCmt = "75";
        public static readonly string CheckedBagHeightInh = "29.5";
        public static readonly string CheckedBagLengthCmt = "90";
        public static readonly string CheckedBagLengthInh = "35.5";
        public static readonly string CheckedBagLinearCmt = "208";
        public static readonly string CheckedBagLinearInh = "81";
        public static readonly string CheckedBagWeightKgm = "32";
        public static readonly string CheckedBagWeightLbr = "70";
        public static readonly string CarryOnBagWidthInh = "9";
        public static readonly string CarryOnBagWidthCmt = "23";
        public static readonly string CarryOnBagHeightCmt = "56";
        public static readonly string CarryOnBagHeightInh = "22";
        public static readonly string CarryOnBagLengthCmt = "36";
        public static readonly string CarryOnBagLengthInh = "14";
        public static readonly string CarryOnBagLinearCmt = "115";
        public static readonly string CarryOnBagLinearInh = "45";
        public static readonly string CarryOnBagWeightKgm = "12";
        public static readonly string CarryOnBagWeightLbr = "26";
        public static readonly string CarryOnBaggageText = "Cabin baggage allowance may vary – please check with the operating airline";
        public static readonly string MediaTypeHeaderValue = "application/json";
        public static readonly string ChannelID = "Channelid";
        public static readonly string OrderCreateEventSubject = "OrderCreate";
        public static readonly string FlightService = "FlightService";
        public static readonly string EligibleCountryBillingState = "ELIGIBLE_COUNTRY_BILLING_STATE";
        public static readonly string SeatDocType = "Y";
        public static readonly string N = "N";
        public static readonly string HK = "HK";
        public static readonly string VS = "VS";
        public static readonly string OrderDocNotFoundErrorCode = "1076";
        public static readonly string[] PaymentTypeEnum = { "CC" };
        public static readonly string[] CCVendor = { "VI", "AX", "CA", "DC", "DX", "TP", "TO", "DS" };
        public static readonly string TO = "TO";
        public static readonly string MO = "MO";
        public static string MASTERCARDTYPE = "CA";
        public static readonly string ISODateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public static int CA_ECISUCCESS_AUTH_U = 0;
        public static int OTHERCARDS_ECISUCCESS_AUTH_U = 7;
        public static int CA_ECISUCCESS = 2;
        public static int CA_ECIATTEMPTED = 1;
        public static int OTHERCARDS_ECISUCCESS = 5;
        public static int OTHERCARDS_ECIATTEMPTED = 6;
        public static int SinglePaxForSplit = 1;
        public static int PaxCountforSplit = 2;
        public static readonly string SeatAddErrors_997 = "S-PpNpq0-1|B-PpNpq0-1|R-PpNpq0-1|FT-PpNpq0-1|S-PpIpU0-1|B-PpIpU0-1|R-PpIpU0-1|FT-PpIpU0-1";
        public static readonly string SeatAddErrors_998 = "S-PpPy0-1|B-PpPy0-1|R-PpPy0-1|FT-PpPy0-1";
        public static readonly string SeatAddErrors_999 = "S-PpPs0-1|B-PpPs0-1|R-PpPs0-1|FT-PpPs0-1|V-PpGs0-1|B-PpGs0-1|R-PpGs0-1|FT-PpGs0-1";
        public static readonly string OrderInstructionId = "OI";
        public static readonly string OrderReshopNameChange = "OrderReshopNameChange";
        public static readonly string OrderChangeReshopHoldBooking = "ReshopHoldBooking";
        public static readonly string OrderReshopDisruption = "OrderReshopDisruption";
        public static string NameChangeServiceDefinitionType = "Name Change";
        public static string NameChangeServiceDefinitionId = "SRVDEF1_NAMCHG";
        public static string NameChangeServiceId = "SV1";
        public static string IFGPaymentTypeCode = "CA";
        public static readonly string InputDateTimeFormat = "yyyy-MM-ddTHH:mm";
        public static string PaymentStatusCode = "SUCCESSFUL";
        public static int HttpStatusErrorCode = 400;
        public static readonly string PrivateFareTypeCode = "758";
        public static readonly string CorporateFareTypeCode = "70G";
        public static readonly string FareTypeCode = "70J";
        public const bool isEasyPay  = false;
        public static readonly string PaymentDocType = "PaymentDoc";
        public static readonly string RefundPaymentDocType = "RefundPaymentDoc";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static readonly string NameSpace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage";
        public static readonly string ARCPaymentCheckEndpoint = "Check";
        public static readonly string ARCPaymentTransactionEndpoint = "Transaction";
        public static readonly string ARCOrderDataEndpoint = "ARC";
        public static readonly string TicketingOn = "Ticketing On";
        public const bool IsSeatReportingEnabled = false;
        public static readonly string TicketDesigCode = "VV81";
        public static readonly string PseudoCityCode = "FJ9";
        public static readonly string SourceSystem = "1P";

        #region ValidationMS
        public static readonly string KeyTypeDirect = "DIRECT";
        public static readonly string KeyTypeIndirect = "INDIRECT";
        #endregion

        #region ConfigMS
        public static readonly string PosCurrency = "PosCurrency";
        public static readonly string UpsellOffers = "UpsellOffers";
        public static readonly string UpsellOffersCacheKey = "EAPI_UPSELL_OFFERS_CACHE_KEY";
        public static readonly string PricingLocationAirportList = "PricingLocationAirportList";
        public static readonly string PricingLocationMapping = "PricingLocationMapping";
        public static readonly string CBA_CLID = "CBA_CLID";
        #endregion

        #region Air4OrderSearchMS
        public static readonly string OrderSearchErrorCodesGeneral = "G035;G009;G001;1088";
        public static readonly string OrderSearchErrorCodesSpecific = "G014;G015;1028;1080";
        #endregion

        #region ReferenceMS
        public static readonly string Air4CurrencyNumber = "AIR4CURRENCYNUMBER";
        public static readonly string CurrencyEndpoint = "Air4CurrencyNumberDetails";
        public static readonly string AirportDetailsEndpoint = "AirportDetail";
        public static readonly string CurrencyExchangeRateEndpoint = "CurrencyExchangeRate";
        #endregion

        #region Penalties
        public static readonly string BeforeDeparture = "Before Departure";
        public static readonly string AfterDeparture = "After Departure";
        public static readonly string TicketRefundFee = "Ticket refund for";
        public const string ChangeAllowedFee = "Change allowed for";
        public const string ChangeAllowedFree = "Change allowed for free";
        public const string ChangeNotAllowed = "Change not allowed";
        public const string ForFee = "For a fee";
        public const string Free = "Free";
        public const string FullyRefundable = "Fully refundable ticket";
        public const string NonRefundable = "Non-refundable";
        public const string PartialRefund = "Full refund";
        public const string RefundFee = "Refund for a fee";
        #endregion

        public static Dictionary<string, Tuple<int, int>> FareFamilyDictionary = new Dictionary<string, Tuple<int, int>>
            {
                {"Economy Light", new Tuple<int, int>(1, 3 ) },
                {"Economy Classic", new Tuple<int, int>(1,  5 ) },
                {"Economy Classic Flex", new Tuple<int, int>( 1, 5 ) },
                {"Economy Delight", new Tuple<int, int>(1 , 6 ) },
                {"Economy Delight Flex", new Tuple<int, int>(1, 5) },
                {"Premium", new Tuple<int, int>(2, 4 ) },
                {"Premium Flex", new Tuple<int, int>(2, 4 ) },
                {"Upper Class", new Tuple<int, int>(3, 2 ) },
                {"Upper Class Flex", new Tuple<int, int>(3, 2 ) }
            };
        public static readonly string IndiaCountryCode = "IN";
        public static readonly string OfflinePaymentTypeCode = "OT";
        public static readonly string OfflineBrandTypeCode = "OT";
        public static readonly string OfflinePayment = "OfflinePayment";
        public static readonly string SuccessPaymentResponse = "SUCCESSFUL";
        public static readonly string BabyMeal = "BBML";
        public static readonly string ChildMeal = "CHML";
        public static readonly string OrderStatusCancelled = "Cancelled";

        public static readonly Dictionary<char, ContactType> ContactTypeMap = new()
        {
            { 'H', ContactType.HOME },
            { 'M', ContactType.MOBILE },
            { 'W', ContactType.WORK},
            { 'E', ContactType.EMAIL}
        };

        public const string TtlExpiryTopicName = "ndcorderttlexpirymessage";
        public const string TtlExpirySubscriptionName = "OrderChangeNotifTtlExpirySubscriber";
        public static readonly string OrderChangeNotifTtlExpiryEventSubject = "OrderChangeNotifTtlExpiry";
    }
}
