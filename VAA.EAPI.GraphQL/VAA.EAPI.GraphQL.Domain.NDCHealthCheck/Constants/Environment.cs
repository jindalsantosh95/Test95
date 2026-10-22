namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Constants
{
    public static class Environment
    {
        public static readonly string OrderHeartBeatUrl = "NDCOrderHeartBeatUrl";
        public static readonly string OfferHeartBeatUrl = "NDCOfferHeartBeatUrl";
        public static readonly string HealthReportUrl = "NDCHealth:NDCHealthReportUrl";
        public static readonly string HealthReportSubKey = "NDCHealth:NDCHealthReportSubKey";
        public static readonly string LowThreshold = "NDCHealth:LowThreshold";
        public static readonly string Threshold = "NDCHealth:Threshold";
        public static readonly string HighThreshold = "NDCHealth:HighThreshold";
        public static readonly string PaymentFacadeHeartBeatUrl = "NDCHealth:PaymentFacadeHeartBeatUrl";
        public static readonly string PaymentFacadeSubKey = "NDCHealth:PaymentFacadeSubKey";
        public static readonly string PaymentFacadeTokenClientId = "NDCHealth:PaymentFacadeTokenClientId";
        public static readonly string PaymentFacadeTokenClientSecret = "NDCHealth:PaymentFacadeTokenClientSecret";
        public static readonly string PaymentFacadeTokenResource = "NDCHealth:PaymentFacadeTokenResource";
        public static readonly string PaymentFacadeTokenUrl = "NDCHealth:PaymentFacadeTokenUrl";
    }
}
