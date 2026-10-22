using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Xml;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{
    public enum OrderActionContext
    {
        [EnumMember(Value = "UPDATE")]
        UPDATE,
        [EnumMember(Value = "REPRICE")]
        REPRICE,
        [EnumMember(Value = "CANCEL")]
        CANCEL
    }

    public static class Constants
    {
        public static readonly string LangCode = "EN";
        public static readonly string VA = "VA";
        public static readonly string DL = "DL";
        public static readonly string SV = "SV";
        public static string OfferClientSecretValue;
        public static string AuthKeySecretValue;
        public static string Permittedcarrier;
        public static string MicroOrderSubscriptionNameValue;
        public static string MaxDocumentSize = "MaxDocumentSize";
        public static readonly string MicroOfferCreateOfferUrl = "MicroOfferCreateOfferUrl";
        public static readonly int TotalNumberOfRetryAttempts = 3;
        public static readonly string MediaTypeHeaderValue = "application/json";
        public static readonly string AuthorizationTokenType = "Bearer";
        public static readonly string ApimSubscription = "Ocp-Apim-Subscription-Key";

        public static string OfferManagmentBearerTokenValue;
        public const string AdultPTCCode = "ADT";
        public const string InfantPTCCode = "INF";
        public const string ChildPTCCode = "CHD";
        public const string PaxPTCCode = "PAX";
        public static readonly string CNNPTCCode = "CNN";
        public const string YoungChildPTCCode = "GBE";
        public static readonly string CabinBaggageType1 = "CAB1";
        public static readonly string CabinBaggageType2 = "CAB2";
        public static readonly string PT = "PT";
        public static readonly string H = "H";
        public static readonly string M = "M";
        public static readonly string S = "S";
        public static readonly string SeatProductCode = "SEAT";
        public static readonly string TicketDocumentType = "T";
        public static readonly string TicketType = "Ticket";
        public static readonly string OriginDestinationRef = "OD";
        public static readonly string TaxEnum = "TaxEnum";
        public static readonly string SellerPartyType = "Seller";
        public static readonly string OwnerCode = "VS";
        public static readonly int AdultAge = 18;
        public static readonly int YoungAge = 15;
        public static readonly int ChildAge = 5;
        public static readonly int InfantAge = 1;
        public static readonly string TECHERROR = "911";
        public static readonly string Vs932TicketPrefix = "VS932";
        public static readonly string BAG = "BAG";
        public static readonly string CAB = "CAB";
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
        public static readonly string TECHERRORMSG = "Internal error (unknown internal technical error)";
        public static readonly string NOTPROCESSED = "NotProcessed";
        public static readonly string PROCESSED = "Processed";
        public static readonly string TAGTEXT = "Internal technical error";
        public static readonly string SUBSCRIPTION = "subscription";
        public static string DocTypeAirShopping = "AirShopping";
        public static string EAPIOrderClientSecret = "";
        public static string DocTypeOrderReshop = "OrderReshop";
        public static string OriginsDestinationsCount = "6";
        public static readonly string NDCNULLRQERRORCODE = "912";
        public static readonly string NDCNULLRQSTATUSCODE = "NotProcessed";
        public static readonly string NDCNULLAIRSHOPPINGRQERRORMSG = "Please complete all mandatory fields in AirShoppingRQ as per given Specifications";
        public static readonly string NDCNULLSERVICELISTRQERRORMSG = "Please complete all mandatory fields in ServiceListRQ as per given Specifications";
        public static readonly string NDCNULLORDERRESHOPRQERRORMSG = "Please complete all mandatory fields in OrderReshopRQ as per given Specifications";
        public static readonly string NDCNULLOFFERPRICERQERRORMSG = "Please complete all mandatory fields in  OfferPriceRQ as per given Specifications";
        public static readonly string NDCNULLSEATAVAILABILITYRQERRORMSG = "Please complete all mandatory fields in  SeatAvailabilityRQ as per given Specifications";
        public static readonly string NDCNULLORDERQUOTERQERRORMSG = "Please complete all mandatory fields in  OrderQuoteRQ as per given Specifications";
        public static readonly string NDCREQUESTVALIDATIONTAGTEXT = "Request Validation Error";
        public static readonly string SerachSortingBasedOn = "customScore";
        public static readonly bool AirShoppingSearchTripDetails = true;
        public static readonly string Traveller = "Traveller";
        public static XmlDocument CabinBagAllowance;
        public static XmlDocument ExitServiceDiscriptionList;
        public static XmlDocument NonExitServiceDiscriptionList;
        public static XmlDocument PieceAllowance;
        // VS Flights BrandID
        public static string DelightBrandID = "VSDT";
        public static string EconomyBrandID = "VSCL";
        public static string PremEconomyBrandID = "VSPE";
        public static string UpperBrandID = "VSUP";
        // Delta Flights BrandID
        public static string EconomyDelightDLBrandID = "DCP";
        public static string EconomyDLBrandID = "MAIN";
        public static string PremiumDLBrandID = "DPPS";
        public static string PremEconomyDLBrandID = "DCP";
        public static string UpperDLBrandID = "D1";

        public static readonly string AllowedHosts = "AllowedHosts";
        public static readonly string genericConfigDocId = "genericConfigDocId";
        public static readonly string agencyRulesDocId = "agencyRulesDocId";
        public static readonly string LocationDocId = "LocationDocId";
        public static readonly string configCollectionId = "configCollectionId";
        public static readonly string applicationInsightKey = "applicationInsightKey";
        public static readonly string cosmosEndpoint = "cosmosEndpoint";
        public static readonly string cosmosAuthKey = "cosmosAuthKeyName";
        public static readonly string cosmosCollectionName = "cosmosCollectionName";
        public static readonly string cosmosSeatCollectionName = "cosmosSeatCollectionName";
        public static readonly string offerMicroBaseURL = "offerMicroBaseURL";
        public static readonly string offerMicroOffersURI = "offerMicroOffersURI";
        public static readonly string offerMicroRepriceURI = "offerMicroRepriceURI";
        public static readonly string productApiBaseURL = "productApiBaseURL";
        public static readonly string productApiSeatMapURI = "productApiSeatMapURI";
        public static readonly string productApiBrandURI = "productApiBrandURI";
        public static readonly string configApiBaseURL = "configApiBaseURL";
        public static readonly string configAPIAgencyConfigURI = "configAPIAgencyConfigURI";
        public static readonly string configAPIGenericConfigURI = "configAPIGenericConfigURI";
        public static readonly string configAPILocationConfigURI = "configAPILocationConfigURI";
        public static readonly string configAPIAirportDetailsConfigURI = "configAPIAirportDetailsConfigURI";
        public static readonly string cosmosDBName = "cosmosDBName";
        public static readonly string cosmosConnectionMode = "cosmosConnectionMode";
        public static readonly string keyVaultURL = "keyVaultURL";
        public static readonly string keyVaultClientID = "keyVaultClientID";
        public static readonly string keyVaultSecretConfigKey = "keyVaultSecretConfigKey";
        public static readonly string azureADEndpoint = "azureADEndpoint";
        public static readonly string offerManagmentSubscriptionKeyName = "offerManagmentSubscriptionKeyName";
        public static readonly string offerManagmentResource = "offerManagmentResource";
        public static readonly string offerManagmentClientID = "offerManagmentClientID";
        public static readonly string offerManagmentClientSecretKey = "offerManagmentClientSecretKey";
        public static readonly string cachingTTL = "cachingTTL";
        public static readonly string ndcRefVersionNumber = "ndcRefVersionNumber";
        public static readonly string airshoppingDocSize = "airshoppingDocSize";
        public static readonly string seatOfferIDMaxLength = "seatOfferIDMaxLength";
        public static readonly string offerMicroDefaultVersion = "offerMicroDefaultVersion";
        public static readonly string offerMicroLatestVersion = "offerMicroLatestVersion";
        public static readonly string offerMicroAgencyWithLatestVersion = "offerMicroAgencyWithLatestVersion";
        public static readonly string refVersionNumber = "refVersionNumber";
        public static readonly string ITAApiName = "ITAApiName";
        public static readonly string configApiBaseSlotURL = "configApiBaseURLSlot";
        public static readonly string offerMicroBaseSlotURL = "offerMicroBaseURLSlot";
        public static readonly string productApiBaseSlotURL = "productApiBaseURLSlot";
        public static readonly string deploymentSlot = "deploymentSlot";
        public static readonly string ApplicationInsightClientID = "ClientID";
        public static readonly string ApplicationInsightPADIS = "PADIS";
        public static readonly string TokenHeaderName = "Authorization";
        public static string BUS_FIRST = "BUS-FIRST";
        public static string CarryOnBag = "CarryOn";
        public static string CheckedBag = "Checked";
        public static string VSMarketed = "Marketed";
        public static string VSOperated = "Operated";
        public static string CabinBaggageText = "CabinBaggageText";
        public static string CabinBaggageJSON;
        public static readonly string PenaltyFreeCHANGES = "ANY TIME CHANGES PERMITTED";
        public static readonly string PenaltyFreeCANCELLATIONS = "ANY TIME CANCELLATIONS PERMITTED";
        public static readonly string PenaltyNotFreeCHANGES = " ANY TIME CHANGES NOT PERMITTED";
        public static readonly string PenaltyNotFreeCANCELLATIONS = "ANY TIME CANCELLATIONS NOT PERMITTED";
        public static readonly string QpxName;
        public static int PNRLength = 6;
        public static readonly string database = "Ndc";
        public static readonly string OrderDBCollection = "OrderDB";
        public static readonly string cosmosEndpointOrder = "cosmosEndpointOrder";
        public static readonly string cosmosAuthKeyOrder = "cosmosAuthKeyNameOrder";
        public static readonly string SeatDocType = "SeatOffer";
        public static string NDCSubscriptionName = "NDCSubscriptionName";
        public static string EAPIOrderReshopQuoteSubscriptionKey = "EAPIOrderReshopQuoteSubscriptionKey";
        public static readonly string Authorization = "Authorization";
        public static string MainOrderTokenId = null;
        public static string ConstantNDCSubscriptionName = "";
        public static string ConstantclientSecret = "";
        public static readonly string clientSecret = "clientSecret";
        public static readonly string clientId = "clientId";
        public static readonly string SkipGdsAgentValidation = "SkipGdsAgentValidation";
        public static readonly string MainOfferresourceUri = "MainOfferresourceUri";
        public static readonly string OrderBaseUrl = "OrderBaseUrl";
        public static readonly string GetOrderUrl = "GetOrderUrl";
        public static readonly string OrderExternalClientId = "OrderExternalClientId";
        public static string OrderExternalClientSecret = "OrderExternalClientSecret";
        public static readonly string OrderExternalResourceUri = "OrderExternalResourceUri";
        public static readonly string OrderExternalSubscriptionKey = "OrderExternalSubscriptionKey";
        public static readonly string offerMicroReshopOffersURI = "offerMicroReshopOffersURI";
        public static readonly string GetOfferItemQuery = "SELECT TOP 1 [Offers] as Offers  FROM c join Offers in c.Offers where Offers.OfferId=@OfferID";
        public static readonly string GetOfferPriceItemQuery = "SELECT * FROM  c where c.id=@OfferID";
        public static readonly string GetReshopItemQuery = "SELECT * FROM c WHERE c.id =@OfferID";
        public static readonly string OfferPriceV2 = "21.3";
        public static readonly string OfferPriceV1 = "18.1";
        public static readonly string IATAV2 = "21.3";
        public static readonly string IATAV1 = "18.1";
        public static readonly string OccupationCodeQ = "Q";
        public static readonly string OccupationCodeF = "F";
        public static readonly string OccupationCodeZ = "Z";
        public static readonly string OccupationCodeO = "O";
        public static readonly string EAPIOrderReshopUrl = "EAPIOrderReshopUrl";
        public static readonly string EAPIFetchOfferDocUrl = "EAPIFetchOfferDocUrl";
        public static readonly string EAPIOrderQuoteUrl = "EAPIOrderQuoteUrl";
        public static readonly string EAPIOrderReshopSubscriptionName = "EAPIOrderSubscriptionName";
        public static string ConstantEAPIOrderReshopSubscriptionName = "";
        public static string ConstantEAPIOrderSearchSubscriptionName = "";
        public static string InvalidRoute = "HAV;";
        public static string ConfigOfferBearerTokenId = null;
        public static string ConstantEAPIReshopClientSecret = "";
        public static string ChannelID = "Channelid";
        public static string NDC = "NDC";
        public static string OrderReshopBearerTokenId = null;
        public static string EAPIOfferClientSecret = "EAPIOfferClientSecret";
        public static string EAPIOrderReshopQuoteResourceUri = "EAPIOrderReshopQuoteResourceUri";
        public static string EAPIOfferClientId = "EAPIOfferClientId";
        public static string EAPI_INVALID_ERROR_RESPONSE_SPECIFIC = "1090;1090A;1091;1092;1092A;1092A i;1092C;1092C i;1092C ii;1093;1093A;1093B;1093C;1093D;1094;1094A;1094B;1094D;1094E;1094I;1095;1095A;1095B;1095C;1095D;1095E;1096;1097;1098;1099;10100;10101;10102;10103;10104;10105;";
        public static string EAPI_INVALID_ERROR_RESPONSE_GENERAL = "G009;G041";
        public static string EAPI_INVALID_ERROR_REPRICE_RESPONSE_SPECIFIC = "10109a;10109b;10109c;10110a;10110b;10111;10112a;10112b;10106;10124;";
        public static string Lavatory = "LA";
        public static string Galley = "G";
        public static string Closet = "CL";
        public static string thresholdDaysAfterBooking = "thresholdDaysAfterBooking";
        public static string thresholdDaysBeforeDeparture = "thresholdDaysBeforeDeparture";
        public static string daysDifferenceDepartureAndBooking = "daysDifferenceDepartureAndBooking";
        public static string hoursDifferenceDepartureAndBooking = "hoursDifferenceDepartureAndBooking";
        public static string FilterOffersAfterDepartureTime = "FilterOffersAfterDepartureTime";
        public static readonly string RegexToValidateNumeric = @"\d+";
        public static readonly string CalendarSearchWarningCode = "76H";
        public static readonly string ITA_Key = "ITAKey";
        public static readonly string ITA_Version = "ITAVersion";
        public static readonly string AgentSubscriptionkey = "agentSubscriptionKey";
        public static readonly string AgentEnvironment = "Environment";
        public static string AgentRegistrationEnvironment = "AgentRegistrationEnvironment";
        public static readonly string EAPIOfferEndpoint = "EAPIOfferEndpoint";
        public static readonly string EAPIOfferResource = "EAPIOfferResource";
        public static string ValidationServiceBearerTokenId = null;
        public static string EAPIValidationServiceSubscriptionName = "EAPIValidationServiceSubscriptionName";
        public static readonly string ValidationServiceEAPIUrl = "ValidationServiceEAPIUrl";
        public static int ACCTFormatMaxLength = 80;
        public static string ACCTFormat = "RM ACCT-NDC {0} {1}/{2}/{3}/{4}";
        public static string KeyTypeDirect = "DIRECT";
        public static string KeyTypeIndirect = "INDIRECT";
        public static string reshopDeptDateforamt = "yyyy-MM-dd";
        public static string AzureAdClientId = "AzureAd:ClientId";
        public static string AzureAdInstance = "AzureAd:Instance";
        public static string AzureAdTenantId = "AzureAd:TenantId";
        public static string AzureAdTokenV1Issuer = "AzureAd:TokenV1Issuer";
        public static readonly string AuthenticationFailureMessage = "You are not authorized!";
        public static readonly string OrderCancelledTagText = "Order Cancelled";
        public static readonly string OrderCancelledErrorCode = "303";
        public static string AllFlownCouponStatus = "FLOWN/USED";
        public static readonly string Current = "Current";
        public static readonly string AdultCustomerID = "ADULT_";
        public static readonly string YoungAdultCustomerID = "YOUNG_";
        public static readonly string ChildCustomerID = "CHILD_";
        public static readonly string InfantCustomerID = "INFANT_";
        public static readonly string PassengerCustomerID = "PASSENGER_";
        public static readonly string BrandMicroServiceEAPIUrl = "EAPIBrandMSEndPoint";
        public static string EAPIBrandMicroServiceSubscriptionName = "EAPIBrandMSOcpKey";
        public static readonly string SkipOfferDocWriteAgencyList = "SkipOfferDocWriteAgencyList";
        public static string ChangePenaltyType = "Change";
        public static string CancellationPenaltyType = "Cancel";
        public static string TIDS = "TIDS";
        public static readonly string AirshoppingEventSubject = "Airshopping";
        public static readonly string NDCVersion = "NDC 21.3.4";
        public static readonly string OfferPriceEventSubject = "OfferPrice";
        public static readonly string OrderReshopEventSubject = "OrderReshop";
        public static readonly string OrderQuoteEventSubject = "OrderQuote";
        public static readonly string SeatAvailabilityEventSubject = "SeatAvailability";
        public static readonly string ServiceListEventSubject = "ServiceList";
        public static readonly string FareswithPenalityCode = "730";
        public static readonly string FareswithoutPenalityCode = "731";
        public static readonly string CheckedBagCode = "13EC";
        public static readonly string PreferredSeatCode = "189C";
        public static readonly string CheckAgentTktAuthority = "CheckAgentTktAuthority";
        public static readonly string PC = "PC";
        public static readonly string SEG = "SEG";
        public static readonly string JOUR = "JOUR";
        public static readonly string PEN = "PEN";
        public static readonly string OD = "OD";
        public static readonly string SD = "SD";
        public static readonly string PENCHANGE = "PEN-CHANGE";
        public static readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm";

        public static Dictionary<string, string> AIRLINENAME = new() {
            { "DL", "Delta" },
            { "VS", "Virgin" }
        };

        public static readonly string VIRGINATLANTICNDC = "Virgin Atlantic NDC";
        public static readonly string NDCVERSIONNUM = "18.1";
        public static readonly string OWNERCODE = "VS";
        public static readonly string INF = "INF";
        public static readonly string ADT = "ADT";
        public static readonly string CHD = "CHD";
        public static readonly string CNN = "CNN";
        public static readonly string BAGA = "BAGA";
        public static readonly string UpperDeck = "Upper";
        public static readonly string LowerOrMainDeck = "Main";
        public static readonly string DATEDMARKETINGSEGMENT = "DMS";
        public static readonly string DATEDOPERATINGLEG = "DOL";
        public static readonly string DATEDMARKETINGREF = "DOS";
        public static readonly string Service = "Service";
        public static readonly string Amenity = "Amenity";
        public static readonly string SRV = "SRV";
        public static readonly string TaxCodeXF = "XF";
        public static readonly string TaxCodeZP = "ZP";
        public static readonly string CarrierName = "Virgin Atlantic Airways";

        public static readonly string UNEVENEXCHANGEWARNINGCODE = "157";
        public static readonly string UNEVENEXCHANGEWARNINGID = "157_1";
        public static readonly string UNEVENEXCHANGEWARNINGDESCTEXT = "Contains offers that might not be changed. Please contact Airline call centre";
        public static readonly string UNEVENEXCHANGEWARNINGSTATUSTEXT = "Processed";
        public static readonly string UNEVENEXCHANGEWARNINGTAGTEXT = "Business Function Not Supported";

        public static readonly string SeatDisclosure = "SeatDisclosure";
        public static readonly string SeatProfile = "SeatProf";
        public static readonly string ServiceDefinitionID = "Seat";
        public static readonly string ExitSeatProductName = "Exit Seat";
        public static readonly string ExitReduceLegroomSeatProductID = "ERL";
        public static readonly string ExitSeatProductId = "EXT";
        public static readonly string NoShow = "NO-SHOW";
        public static readonly string BeforeDeparture = "Before Departure";
        public static readonly string AfterDeparture = "After Departure";
        public const string FullyRefundable = "Fully refundable ticket";
        public const string PartialRefund = "Full refund";
        public const string NonRefundable = "Non-refundable";
        public const string RefundFee = "Refund for a fee";
        public const string TicketRefundFee = "Ticket refund for";
        public const string ChangeAllowedFee = "Change allowed for";
        public const string ChangeNotAllowed = "Change not allowed";
        public const string ForFee = "For a fee";
        public const string Free = "Free";
        public const string ChangeAllowedFree = "Change allowed for free";
        public static readonly string Flex = "Flex";
        public static readonly string DirectFlightIndicator = "DIRECT";
        public static readonly string Meals = "meals";
        public static readonly string Disability = "disability";


        public static readonly string FareTypeCode = "70J";
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

        static Constants()
        {
            PieceAllowance = new XmlDocument();
            CabinBagAllowance = new XmlDocument();
            ExitServiceDiscriptionList = new XmlDocument();
            NonExitServiceDiscriptionList = new XmlDocument();

        }
    }
}
