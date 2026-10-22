namespace VAA.EAPI.NDC.Order.Constants
{
    public static class Environment
    {
        #region ValidationMS
        public static readonly string ValidationMSUrl = "ValidationMSUrl";
        public static readonly string ValidationMSAgencyOCNDetailsUrl = "ValidationMSAgencyOCNDetailsUrl";
        public static readonly string ValidationMSSubscriptionKey = "ValidationMSSubscriptionKey";
        public static readonly string AgentRegistrationEnvironment = "AgentRegistrationEnvironment";
        #endregion

        #region OfferMS
        public static readonly string OfferMSDocEndpoint = "OfferMSDocEndpoint";
        public static readonly string OfferMSSubscriptionKey = "OfferMSSubscriptionKey";
        public static readonly string CancelReshopOfferMSDocEndpoint = "CancelReshopOfferMSDocEndpoint";
        #endregion

        #region ConfigMS    
        public static readonly string CurrencyDBKey = "CurrencyDBKey";
        public static readonly string CurrencyDBTableName = "CurrencyDBTableName";
        public static readonly string ReshopFareFamilyDBTableName = "ReshopFareFamilyDBTableName";
        public static readonly string UpsellOffersDBTableName = "UpsellOffersDBTableName";
        public static readonly string PricingLocationDBTableName = "PricingLocationDBTableName";
        public static readonly string AirportListDBTableName = "AirportListDBTableName";
        public static readonly string FareFamilyListDBTableName = "FareFamilyListDBTableName";
        public static readonly string ConfigurationMSPricingLocationUrl = "ConfigurationMSPricingLocationUrl";
        public static readonly string ConfigurationPosCurrencyMSUrl = "ConfigurationPosCurrencyMSUrl";
        public static readonly string ConfigurationMSSubscriptionKey = "ConfigurationMSSubscriptionKey";
        public static readonly string ConfigurationMSPOSCurrencyEndpoint = "ConfigurationMSPOSCurrencyEndpoint";
        public static readonly string ConfigurationMSBSPMarketPaymentEndpoint = "ConfigurationMSBSPMarketPaymentEndpoint";
        public static readonly string ConfigurationMSBSPMarketCurrencyEndpoint = "ConfigurationMSBSPMarketCurrencyEndpoint";
        public static readonly string ConfigurationMSCityCodeEndpoint = "ConfigurationMSCityCodeEndpoint";


        #endregion

        #region BrandMS
        public static readonly string BrandMSUrl = "BrandMSUrl";
        public static readonly string BrandMSSubscriptionKey = "BrandMSSubscriptionKey";
        #endregion

        #region SeatMS
        public static readonly string SeatMSUrl = "SeatMSUrl";
        public static readonly string SeatMSSubscriptionKey = "SeatMSSubscriptionKey";
        public static readonly string SeatMSofferUrl = "SeatMSofferUrl";
        public static readonly string SeatMSSeatFulfilmentUrl = "SeatMSSeatFulfilmentUrl";
        public static readonly string SeatMSRetrieveSeatMapUrl = "SeatMSRetrieveSeatMapUrl";
        #endregion

        #region FlightServicesMS
        public static readonly string FlightServicesMSUrl = "FlightServicesMSUrl";
        public static readonly string FlightServicesMSSubscriptionKey = "FlightServicesMSSubscriptionKey";
        #endregion

        #region OrderDataMS
        public static readonly string OrderDataMSEndpoint = "OrderDataMSEndpoint";
        public static readonly string OrderDataMSOrderListEndpoint = "OrderDataMSOrderListEndpoint";
        public static readonly string OrderDataMSSubscriptionKey = "OrderDataMSSubscriptionKey";
        public static readonly string OrderDataMSUpdateEndpoint = "OrderDataMSUpdateEndpoint";
        #endregion

        #region OrderSearchMS
        public static readonly string OrderSearchMSUrl = "OrderSearchMSUrl";
        public static readonly string OrderSearchMSSubscriptionKey = "OrderSearchMSSubscriptionKey";
        public static readonly string PublishOrderDocUrl = "PublishOrderDocUrl";
        public static readonly string OrderSearchMSOrderHistoryUrl = "OrderSearchMSOrderHistoryUrl";

        #endregion

        #region NdcQueueMgmt
        public static readonly string NdcQueueMgmtFetchDuplicatePnrsUrl = "NdcQueueMgmtFetchDuplicatePnrsUrl";
        public static readonly string NdcQueueMgmtRemoveDuplicatePnrsUrl = "NdcQueueMgmtRemoveDuplicatePnrsUrl";
        public static readonly string NdcQueueMgmtSubscriptionKey = "NdcQueueMgmtSubscriptionKey";
        #endregion

        #region Air4OrderSearchMS
        public static readonly string Air4OrderSearchMSUrl = "Air4OrderSearchMSUrl";
        public static readonly string Air4OrderSearchMSSubscriptionKey = "Air4OrderSearchMSSubscriptionKey";
        #endregion

        #region OrderChange
        public static readonly string OrderChangeMSUrl = "OrderChangeMSUrl";
        public static readonly string OrderChangeMSSubscriptionKey = "OrderChangeMSSubscriptionKey";
        public static readonly string OrderCancelMSUrl = "OrderCancelMSUrl";
        public static readonly string OrderCancelMSSubscriptionKey = "OrderCancelMSSubscriptionKey";
        public static readonly string OrderMgmtMSUrl = "OrderMgmtMSUrl";
        public static readonly string OrderMgmtMSSubscriptionKey = "OrderMgmtMSSubscriptionKey";
        public static readonly string ManageCustomerMSUrl = "ManageCustomerMSUrl";
        public static readonly string SplitPnrMSUrl = "SplitPnrMSUrl";
        public static readonly string PublishSplitOrderHistoryUrl = "PublishSplitOrderHistoryUrl";
        #endregion

        #region OrderMS
        public static readonly string OrderMSSubscriptionKey = "OrderMSSubscriptionKey";
        public static readonly string OrderMSCreateUrl = "OrderMSCreateUrl";
        public static readonly string OrderMSResponseUrl = "OrderMSResponseUrl";
        #endregion

        #region OrderTicketMS
        public static readonly string OrderTicketMSSubscriptionKey = "OrderTicketMSSubscriptionKey";
        public static readonly string OrderTicketMSUrl = "OrderTicketMSUrl";
        #endregion

        #region ReferenceMS
        public static readonly string ReferenceMSUrl = "ReferenceMSUrl";
        public static readonly string ReferenceMSSubscriptionKey = "ReferenceMSSubscriptionKey";
        #endregion

        public static readonly string EligibleCountryBillingState = "EligibleCountryBillingState";
        public static readonly string SkipGDSAgentValidation = "SkipGDSAgentValidation";

        #region PaymentMS
        public static readonly string PaymentMSAuthorizeUrl = "PaymentMSAuthorizeUrl";
        public static readonly string PaymentMSSubscriptionKey = "PaymentMSSubscriptionKey";
        public static readonly string PaymentMSCaptureUrl = "PaymentMSCaptureUrl";
        public static readonly string PaymentMSRefundUrl = "PaymentMSRefundUrl";
        public static readonly string PaymentMSRetrieveUrl = "PaymentMSRetrieveUrl";
        public static readonly string PaymentMSSplitPnrUrl = "PaymentMSSplitPnrUrl";
        public static readonly string CreatePaymentDocumentUrl = "CreatePaymentDocumentUrl";
        #endregion

        #region PaymnetIFGMS
        public static readonly string PaymentIFGMSAuthorizeUrl = "PaymentIFGMSAuthorizeUrl";
        public static readonly string PaymentIFGMSOrderUrl = "PaymentIFGMSOrderUrl";
        public static readonly string PaymentIFGMSExchangeUrl = "PaymentIFGMSExchangeUrl";
        public static readonly string PaymentIFGMSSubscriptionKey = "PaymentIFGMSSubscriptionKey";
        public static readonly string PaymentIFGMSOrderRefundUrl = "PaymentIFGMSOrderRefundUrl";
        public static readonly string PaymentIFGMSAuthorizeCancelUrl = "PaymentIFGMSAuthorizeCancelUrl";
        #endregion

        #region EmailMS
        public static readonly string EmailMSSubscriptionKey = "EmailMSSubscriptionKey";
        public static readonly string EmailMSSendOrderEmailUrl = "EmailMSSendOrderEmailUrl";
        #endregion

        #region OrderNotificationMS
        public static readonly string OrderNotificationMSUrl = "OrderNotificationMSUrl";
        public static readonly string OrderNotificationMSSubscriptionKey = "OrderNotificationMSSubscriptionKey";
        #endregion

        #region InRule
        public static readonly string InRuleEnabled = "InRuleEnabled";
        public static readonly string InRuleUrl = "InRuleUrl";
        public static readonly string InRuleSubscriptionKey = "InRuleSubscriptionKey";
        public static readonly string InRuleScope = "InRuleScope";
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
        public static readonly string OrderTicketMSHeartBeatUrl = "OrderTicketMSHeartBeatUrl";
        public static readonly string OrderMSHeartBeatUrl = "OrderMSHeartBeatUrl";
        public static readonly string OrderMgmtMSHeartBeatUrl = "OrderMgmtMSHeartBeatUrl";
        public static readonly string Air4OrderMgmtMSHeartBeatUrl = "Air4OrderMgmtMSHeartBeatUrl";
        public static readonly string EmailMSHeartBeatUrl = "EmailMSHeartBeatUrl";
        public static readonly string PaymentMSHeartBeatUrl = "PaymentMSHeartBeatUrl";
        public static readonly string OrderNotificationMSHeartBeatUrl = "OrderNotificationMSHeartBeatUrl";
        public static readonly string IFGPaymentMSHeartBeatUrl = "IFGPaymentMSHeartBeatUrl";
        public static readonly string ARCPaymentMSHeartBeatUrl = "ARCPaymentMSHeartBeatUrl";

        #endregion

        public const string OrderListFilterDateRange = "OrderListFilterDateRange";
        public static readonly string TicketStockNumber = "TicketStockNumber";
        public static readonly string IFGAgentFopLookUpUrl = "IFGAgentFopLookUpUrl";
        public static readonly string IFGAgentFOPLookUpSubscriptionKey = "IFGAgentFOPLookUpSubscriptionKey";
        public static readonly string ARCPaymentMSSubscriptionKey = "ARCPaymentMSSubscriptionKey";
        public static readonly string ARCPaymentMSUrl = "ARCPaymentMSUrl";

        public static readonly string isIFGIntegrated = "IsIfgIntegrated";


    }
}
