namespace VAA.EAPI.Microservices.Retail.Trip.Constant
{
    public static class Environment
    {
        public static readonly string x_air4_mytrips_test = "x-air4_mytrips_test";
        public static readonly string DlMyTripsUri = "DlMyTripsUri";
        public static readonly string DlMessageCentreUri = "DlMessageCentreUri";
        public static readonly string DlPassengerInfoUri = "DlPassengerInfoUri";
        public static readonly string DlUpsellOfffersUri = "DlUpsellOfffersUri";
        public static readonly string DlEncryptionServiceUri = "DlEncryptionServiceUri";

        //Deeplink Configurations
        public static readonly string TravelInsuranceDeeplinkUrl = "TravelInsuranceDeeplinkUrl_{0}";
        public static readonly string FlightDisruptionUrl = "FlightDisruptionUrl";
        public static readonly string BaggageAllowanceUrl = "BaggageAllowanceUrl";
        public static readonly string BookingIdJwtKey = "BookingIdJwtKey";
        public static readonly string ManageMyBookingUrl = "ManageMyBookingUrl";
        public static readonly string ManageMyHolidayUrl = "ManageMyHolidayUrl";
        public static readonly string RedisConnectionString = "RedisConnectionString";
        public static readonly string DeeplinkExpiryLimitInMinutes = "DeeplinkExpiryLimitInMinutes";
        public static readonly string ProxyDeeplinkURL = "ProxyDeeplinkURL";
        public static readonly string IROP_FAFURL = "IROP_FAFURL";
        public static readonly string FlightCancelUrl = "FlightCancelUrl";
        public static readonly string POPSMealUrl = "POPSMealUrl";
        public static readonly string POPSTokenKey = "POPSTokenKey";

        //DL Private endpoint configurations
        public static readonly string DeltaAir4TokenEndpoint = "DeltaAir4TokenEndpoint";
        public static readonly string DlMyTripsPrivateUri = "DlMyTripsPrivateUri";
        public static readonly string DlMessageCentrePrivateUri = "DlMessageCentrePrivateUri";
        public static readonly string DlPassengerInfoPrivateUri = "DlPassengerInfoPrivateUri";
        public static readonly string DlUpsellOfffersPrivateUri = "DlUpsellOfffersPrivateUri";
        public static readonly string DlEncryptionServicePrivateUri = "DlEncryptionServicePrivateUri";
        public static readonly string SwitchToPrivateEndpoints = "SwitchToPrivateEndpoints";

        //Plus grade configurations
        public static readonly string PlusGradeApiKey = "PlusGradeApiKey";
        public static readonly string PlusGradeApiUri = "PlusGradeApiUri";
        public static readonly string PlusGradePartnerId = "PlusGradePartnerId";

    }
}