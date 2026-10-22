namespace VAA.EAPI.Microservices.Flight.Services.Constants
{
    public class Application
    {
        public static readonly string SSRTableName = "FlightService";
        public static readonly string CacheKey = "SSRCachekey";
        public static readonly string NdcChannel = "NDC";
        public static readonly string IsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public static readonly string FlightServicveDocType = "FlightServicveOffers";
        public static readonly string SqlQueryFlightServiceOfferSearch = "SELECT * FROM c where c.id = @Id";
        public static readonly string ZuluDateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

        //Delta SSR service
        public static readonly string TransactionId = "transactionid";
        public static readonly string AppId = "appid";
        public static readonly string ChannelId = "channelId";
        public static readonly string DFMobileChannelId = "DFMOBILE";
        public static readonly string DFAppId = "D2";
        public static readonly string AirlineCode = "VS";
    }
}
