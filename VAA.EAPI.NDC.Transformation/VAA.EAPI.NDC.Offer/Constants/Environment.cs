namespace VAA.EAPI.NDC.Offer.Constants
{
    public static class Environment
    {
        public static readonly string AzureADEndpoint = "AzureADEndpoint";

        public static readonly string OrderSearchEndpoint = "OrderSearch";

        public static readonly string PublishOrderDocEndpoint = "PublishOrderDoc";

        #region ValidationMS
        public static readonly string ValidationMSUrl = "ValidationMSUrl";
        public static readonly string ValidationMSSubscriptionKey = "ValidationMSSubscriptionKey";
        public static readonly string AgentRegistrationEnvironment = "AgentRegistrationEnvironment";
        #endregion  

        #region OfferMS
        public static readonly string AirshoppingOfferEndpoint = "AirshoppingOfferEndpoint";
        public static readonly string CalendarSearchEndpoint = "CalendarSearchEndpoint";
        public static readonly string OfferMSSubscriptionKey = "OfferMSSubscriptionKey";
        public static readonly string OfferMSSearchEndpoint = "OfferSearchEndPoint";
        public static readonly string OfferPriceMSEndpoint = "OfferPriceMSEndpoint";
        public static readonly string OrderReshopEndpoint = "OrderReshopEndpoint";
        public static readonly string OrderQuoteEndpoint = "OrderQuoteEndpoint";
        public static readonly string OfferMSReshopCancelEndpoint = "OfferMSReshopCancelEndpoint";
        public static readonly string OfferMSReshopNameCorrectionEndpoint = "OfferMSReshopNameCorrectionEndpoint";
        public static readonly string ReshopDisruptionTicketValidityWindowInDays = "ReshopDisruptionTicketValidityWindowInDays";
        #endregion

        #region ConfigMS
        public static readonly string ConfigurationMSBSPMarketCurrencyEndpoint = "ConfigurationMSBSPMarketCurrencyEndpoint";
        public static readonly string ConfigurationMSITAFareTypeCodeEndpoint = "ConfigurationMSITAFareTypeCodeEndpoint";
        public static readonly string ConfigurationMSBSPMarketPaymentEndpoint = "ConfigurationMSBSPMarketPaymentEndpoint";
        public static readonly string ConfigurationMSPccCurrencyEndpoint = "ConfigurationMSPccCurrencyEndpoint";
        public static readonly string ConfigurationMSReshopFareFamilyEndpoint = "ConfigurationMSReshopFareFamilyEndpoint";
        public static readonly string ConfigurationMSPricingLocationEndpoint = "ConfigurationMSPricingLocationEndpoint";
        public static readonly string ConfigurationMSPOSCurrencyEndpoint = "ConfigurationMSPOSCurrencyEndpoint";
        public static readonly string ConfigurationMSUpsellOffersEndpoint = "ConfigurationMSUpsellOffersEndpoint";
        public static readonly string ConfigurationMSFareFamilyEndpoint = "ConfigurationMSFareFamilyEndpoint";
        public static readonly string ConfigurationMSTtlRulesEndpoint = "ConfigurationMSTtlRulesEndpoint";
        public static readonly string ConfigurationMSTtlEnabledCityEndpoint = "ConfigurationMSTtlEnabledCityEndpoint";
        public static readonly string ConfigurationMSClidEndpoint = "ConfigurationMSClidEndpoint";

        public static readonly string ConfigurationMSOcpKey = "ConfigurationMSOcpKey";
        #endregion

        #region BrandMS
        public static readonly string BrandMSUrl = "BrandMSUrl";
        public static readonly string BrandMSSubscriptionKey = "BrandMSSubscriptionKey";
        #endregion

        #region SeatMS
        public static readonly string SeatMSUrl = "SeatMSUrl";
        public static readonly string SeatMSSubscriptionKey = "SeatMSSubscriptionKey";
        #endregion

        #region FlightServiceMS
        public static readonly string FlightServicesMSUrl = "FlightServicesMSUrl";
        public static readonly string FlightServicesMSSubscriptionKey = "FlightServicesMSSubscriptionKey";
        #endregion

        #region OrderDataMS
        public static readonly string OrderDataMSEndpoint = "OrderDataMSEndpoint";
        public static readonly string OrderDataMSSubscriptionKey = "OrderDataMSSubscriptionKey";
        #endregion

        #region Air4OrderSearchMS
        public static readonly string Air4OrderSearchMSUrl = "Air4OrderSearchMSUrl";
        public static readonly string Air4OrderSearchMSSubscriptionKey = "Air4OrderSearchMSSubscriptionKey";
        #endregion

        #region OrderSearchMS
        public static readonly string OrderSearchMSUrl = "OrderSearchMSUrl";
        public static readonly string OrderSearchMSSubscriptionKey = "OrderSearchMSSubscriptionKey";
        #endregion

        #region ReferenceMS
        public static readonly string ReferenceMSAirportDetailsEndpoint = "ReferenceMSAirportDetailsEndpoint";
        public static readonly string ReferenceMSOcpKey = "ReferenceMSOcpKey";
        #endregion

        #region HeartBeatUrls
        public static readonly string ValidationMSHeartBeatUrl = "ValidationMSHeartBeatUrl";
        public static readonly string ConfigMSHeartBeatUrl = "ConfigMSHeartBeatUrl";
        public static readonly string OfferMSHeartBeatUrl = "OfferMSHeartBeatUrl";
        public static readonly string BrandMSHeartBeatUrl = "BrandMSHeartBeatUrl";
        public static readonly string SeatMSHeartBeatUrl = "SeatMSHeartBeatUrl";
        public static readonly string FlightServicesMSHeartBeatUrl = "FlightServicesMSHeartBeatUrl";
        public static readonly string OrderDataMSHeartBeatUrl = "OrderDataMSHeartBeatUrl";
        public static readonly string Air4OrderSearchMSHeartBeatUrl = "Air4OrderSearchMSHeartBeatUrl";
        public static readonly string OrderSearchMSHeartBeatUrl = "OrderSearchMSHeartBeatUrl";
        public static readonly string ReferenceMSHeartBeatUrl = "ReferenceMSHeartBeatUrl";
        #endregion
    }
}
