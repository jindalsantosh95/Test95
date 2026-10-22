namespace VAA.EAPI.Microservices.Retail.Reference.Constants
{
    public class Application
    {
        public static string SQLRefDbMIToken;
        public static readonly string CountryCodeCacheKey = "EAPI_COUNTRYCODE_CACHE_KEY";
        public static readonly string PartnerCodeCacheKey = "EAPI_PARTNERCODE_CACHE_KEY";
        public static readonly string ProductCacheKey = "EAPI_PRODUCT_CACHE_KEY";
        public static readonly string TierThresholdCacheKey = "EAPI_TIERTHRESHOLD_CACHE_KEY";
        public static readonly string SPQueryinRefDB = "EXEC [dbo].[SP_GetMemberAccountReferenceData]";
        public static readonly string DistanceSPQueryinRefDB = "EXEC [STG].[GetDistanceMiles] @OriginCountryCode, @DestinationCountryCode";
        public static readonly string GetBookingClass = "EXEC [STG].[GetBookingClass] @BookingClass, @FlownClass, @ActivityDate, @OperatedAirlineCode, @ListFareValue, @ListTicketValue, @ListPaxValue, @Origin, @Destination";
        public static readonly string GetBookingClassNew = "EXEC [STG].[GetBookingClassNew] @BookingClass, @FlownClass, @OperatedAirlineCode, @ActivityDate, @Origin, @Destination";

        public static readonly string GetCabinClassNonVSRetro = "EXEC [STG].[GetCabinClass] @operatedAirlineCode, @flightActivityDate, @operatingBookingClass, @marketingBookingClass, @originAirportCode, @destinationAirportCode";
        public static readonly string DistanceSPInRuleRefDB = "EXEC [STG].[GetDistanceOfRouteForInRule] @OriginCountryCode, @DestinationCountryCode";

        public static readonly string DataTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public static readonly string CurrencyConversionBlobName = "CurrencyConversion.json";
    }
}
