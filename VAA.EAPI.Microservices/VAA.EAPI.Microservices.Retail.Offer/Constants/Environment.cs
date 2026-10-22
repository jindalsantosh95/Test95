namespace VAA.EAPI.Microservices.Retail.Offer.Constants
{
    public class Environment
    {
        public static readonly string OpenApiDescription = "OpenApi__Description";
        public static readonly string OpenApiTokenUrl = "OpenApi__TokenUrl";
        public static readonly string OpenApiScope = "OpenApi__Scope";
        public static readonly string ItaServiceKey = "ItaServiceKey";
        public static readonly string ItaServiceVersion = "ItaServiceVersion";
        public static readonly string RewardSeatAvailabilityItaVersion = "RewardSeatAvailability:ItaVersion";
        public static readonly string ItaServiceUrl = "ItaServiceUrl";
        public static readonly string ItaServiceApi = "ItaServiceApi";
        public static readonly string ItaDataset = "ItaDataset";
        public static readonly string ItaServiceAPIMSubKey = "ItaServiceAPIMSubKey";
        public static readonly string ItaReshopApi = "ItaReshopApi";
        public static readonly string ItaReshopApiKey = "ItaReshopApiKey";
        public static readonly string ItaReshopApiVersion = "ItaReshopApiVersion";
        public static readonly string Air4PnrCommonEndPoint = "Air4PnrCommonEndPoint";
        public static readonly string TicketingProcessV3EndPoint = "TicketingProcessV3EndPoint";
        public static readonly string BookingProcessV3EndPoint = "BookingProcessV3EndPoint";
        public static readonly string Air4PassengerV4EndPoint = "Air4PassengerV4EndPoint";
        public static readonly string Air4SeatV8EndPoint = "Air4SeatV8EndPoint";
        public static readonly string CreditCardV2EndPoint = "CreditCardV2EndPoint";
        public static readonly string RemarksV3EndPoint = "RemarksV3EndPoint";
        public static readonly string RFCWindowInHours = "RFCWindowInHours";
        public static readonly string ApplicationId = "ApplicationId";
        public static readonly string DebugOptions = "DebugOptions";
        public static readonly string TestLab = "TestLab";
        public static readonly string PoolType = "PoolType";
        public static readonly string AppChannelName = "AppChannelName";
        public static readonly string ReferenceDataMicroServiceDBKey = "ReferenceDataMicroServiceDBKey";
        public static readonly string CommonCachingSlidingExpirationInDay = "CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "CommonCachingAbsoluteExpirationInDay";
        public static readonly string AmenitiesTableName = "AmenitiesTableName";
        public static readonly string OfferExpiryLimitInMinutes = "OfferExpiryLimitInMinutes";
        public static readonly string DeltaOfferExpiryLimitInMinutes = "DeltaOfferExpiryLimitInMinutes";
        public static readonly string Air4HostContextEndPoint = "Air4HostContextEndPoint";
        public static readonly string Air4SchedulesAvailabilityEndPoint = "Air4SchedulesAvailabilityEndPoint";
        public static readonly string Air4ItliteApplicationId = "Air4ItliteApplicationId"; // TODO - Need to move in Azure Storage 
        public static readonly string Air4ItlitePoolType = "Air4ItlitePoolType"; // TODO - Need to move in Azure Storage 
        public static readonly string HostContextLocation = "HostContextLocation";  // TODO - Need to move in Azure Storage 
        public static readonly string HostContextTimeoutInactivity = "HostContextTimeoutInactivity";
        public static readonly string HostContextTimeoutSession = "HostContextTimeoutSession";
        public static readonly string ItaQpx = "ItaQpx";
        public static readonly string ItaServiceReshopKey = "ItaServiceReshopKey";
        public static readonly string ItaServiceFlexiDateKey = "ItaServiceFlexiDateKey";
        public static readonly string DaysDifferenceDepartureAndBooking = "DaysDifferenceDepartureAndBooking";
        public static readonly string ThresholdDaysBeforeDeparture = "ThresholdDaysBeforeDeparture";
        public static readonly string ThresholdDaysAfterBooking = "ThresholdDaysAfterBooking";
        public static readonly string HoursDifferenceDepartureAndBooking = "HoursDifferenceDepartureAndBooking";
        public static string CabinBaggageText = "CabinBaggageText";

        public static readonly string DlOfferAuthenticationUrl = "DlOfferAuthenticationUrl";
        public static readonly string DlOfferClientId = "DlOfferClientId";
        public static readonly string DlOfferClientSecret = "DlOfferClientSecret";
        public static readonly string DlOfferUri = "DlOfferUri";
        public static readonly string DlAwsOfferGqlUri = "DlAwsOfferGqlUri";
        public static readonly string DlApiApplicationId = "DlApiApplicationId";
        public static readonly string DlApiChannelId = "DlApiChannelId";
        public static readonly string DlOfferFareRulesUri = "DlOfferFareRulesUri";
        public static readonly string DlResultPageNumber = "DlResultPageNumber";
        public static readonly string DlResultCountPerPage = "DlResultCountPerPage";
        public static readonly string DlOfferPagingEnable = "DlOfferPagingEnable";
        public static readonly string IgnoreFlexFares = "IgnoreFlexFares";
        public static readonly string EnableOfferBaselineFareDifferences = "EnableOfferBaselineFareDifferences";

        public static readonly string DlAwsOfferAuthenticationUrl = "DlAwsOfferAuthenticationUrl";
        public static readonly string DlAwsOfferClientId = "DlAwsOfferClientId";
        public static readonly string DlAwsOfferClientSecret = "DlAwsOfferClientSecret";
        public static readonly string DlAwsOfferUri = "DlAwsOfferUri";
        public static readonly string SwitchToAws = "SwitchToAws";
        public static readonly string DlAwsOfferFareRulesUri = "DlAwsOfferFareRulesUri";
        public static readonly string SwitchToAwsGraphql = "SwitchToAwsGraphql";
        public static readonly string RedisConnectionString = "RedisConnectionString";
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";

        public static readonly string ReferenceDataBlobStorageEndPoint = "ReferenceDataBlobStorageEndPoint";
        public static readonly string ReferenceDataBlobContainerName = "ReferenceDataBlobContainerName";
        public static readonly string ReferenceDataBlobSasKey= "ReferenceDataBlobSasKey";
        public static readonly string AirportLocationBlobName = "AirportLocationBlobName";
        public static readonly string CabinDataBlobName = "CabinDataBlobName";
        public static readonly string CabinBagAllowanceBlobName = "CabinBagAllowanceBlobName";
        public static readonly string PieceAllowanceBlobName = "PieceAllowanceBlobName";
        public static readonly string PriceClassBlobName = "PriceClassBlobName";
        public static readonly string ItaFareFamilyBlobName = "ItaFareFamilyBlobName";
        public static readonly string RewardSeatAvailabilityMarketPolicyVersion = "RewardSeatAvailability:MarketPolicyVersion";
        public static readonly string RewardSeatAvailabilityFareFamilyMappingVersion = "RewardSeatAvailability:FareFamilyMappingVersion";
        public static readonly string RewardSeatAvailabilityMarkets = "RewardSeatAvailability:Markets";
        public static readonly string RewardSeatMarketCountryIssued = "CountryIssued";
        public static readonly string RewardSeatMarketCountrySold = "CountrySold";
        public static readonly string RewardSeatMarketCurrency = "Currency";
        public static readonly string RewardSeatMarketSalesCity = "SalesCity";
        public static string RewardSeatEconomyBookingCode(string carrier) => $"RewardSeatAvailability:BookingCodes:{carrier}:Economy";
        public static string RewardSeatPremiumBookingCode(string carrier) => $"RewardSeatAvailability:BookingCodes:{carrier}:Premium";
        public static string RewardSeatUpperBookingCode(string carrier) => $"RewardSeatAvailability:BookingCodes:{carrier}:Upper";
    }
}
