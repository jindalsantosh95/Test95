namespace VAA.EAPI.Microservices.Flight.Services.Constants
{
    public class Environment
    {
        public static readonly string ReferenceDataMicroServiceDBKey = "MicroServiceDBKey";
        public static readonly string CommonCachingSlidingExpirationInDay = "CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "CommonCachingAbsoluteExpirationInDay";
        public static readonly string FlightServiceDBEndpoint = "FlightServiceDBEndpoint";
        public static readonly string FlightServiceDB = "FlightServiceDB";
        public static readonly string FlightServiceDBCollection = "FlightServiceDBCollection";
        public static readonly string OfferExpiryLimitInMinutes = "OfferExpiryLimitInMinutes";
        public static readonly string RedisConnectionString = "RedisConnectionString";

        //Delta SSR Service
        public static readonly string DlMyTripsID = "x-air4_mytrips_test";
        public static readonly string DlMySSRUri = "DlSSRUri";
        public static readonly string ApplicationId = "ApplicationId";
        public static readonly string DebugOptions = "DebugOptions";
        public static readonly string TestLab = "TestLab";
        public static readonly string Air4PnrCommonEndPoint = "Air4PnrCommonEndPoint";
        public static readonly string SpecialServiceRequestV4EndPoint = "SpecialServiceRequestV4EndPoint";

        //DL Private endpoint configurations
        public static readonly string DeltaAir4TokenEndpoint = "DeltaAir4TokenEndpoint";
        public static readonly string DlMySSRPrivateUri = "DlMySSRPrivateUri";
        public static readonly string SwitchToPrivateEndpoints = "SwitchToPrivateEndpoints";
        public static readonly string DeltaClientIdValue = "DeltaClientIdValue";
        public static readonly string DeltaClientSecret = "DeltaClientSecret";
    }
}
