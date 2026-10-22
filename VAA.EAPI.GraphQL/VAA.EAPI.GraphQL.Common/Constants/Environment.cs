namespace VAA.EAPI.GraphQL.Common.Constants
{
    public static class Environment
    {
        public static readonly string AB2CAuthority = "AB2C:Authority";
        public static readonly string AB2CTenant = "AB2C:TenantId";
        public static readonly string AB2COpenIdConnection = "AB2C:AB2COpenIdConnection";
        public static readonly string AB2CAudience = "AB2C:AB2CAudience";
        public static readonly string AADAuthority = "AAD:Authority";
        public static readonly string AADTenant = "AAD:TenantId";
        public static readonly string AADTokenV1Issuer = "AAD:TokenV1Issuer";
        public static readonly string EntraTenant = "AAD:EntraTenantId";
        public static readonly string OpenApiAuthTokenUrl = "https://login.microsoft.com/{ADTenantId}/oauth2/v2.0/token";
        public static readonly string OpenApiScopes = "https://{TenantID}.onmicrosoft.com/{ApplicationID}/Scope";
        public static readonly string OperationsVisibilityMode = "OperationsVisibilityMode";
        public static readonly string AllowIntrospection = "AllowIntrospection";
        public static readonly string ResponseUrlParName = "location";
        public static readonly string EAPIGraphAppID = "EAPIGraph:AppID";
        public static readonly string EAPIGraphSecret = "EAPIGraph:ClientSecret";
        public static readonly string CommonEAPIOcpAPIMKey = "CommonEAPIOcpAPIMKey";
        public static readonly string CustomerAppId = "Customer:AppId";
        public static readonly string CustomerOcpKey = "Customer:OcpSubID";
        public static readonly string CustomerUrlGetbyCustID = "Customer:UrlGetbyCustID";
        public static readonly string CustomerUrlGetByMemNum = "Customer:UrlGetByMemNum";
        public static readonly string BasicCustomerMSUrl = "Customer:BasicEnrolmentUrl";
        public static readonly string VAAccountCustomerMSUrl = "Customer:VACAccountEnrolmentUrl";
        public static readonly string ContactHistoryRetrievalUrl = "Customer:ContactHistoryRetrievalUrl";
        public static readonly string AddressHistoryRetrievalUrl = "Customer:AddressHistoryRetrievalUrl";
        public static readonly string CustomerDocByCustIdUrl = "Customer:CustomerDocByCustIdUrl";
        public static readonly string CustomerMSURLForValidateMemberStatus = "CustomerMSURLForValidateMemberStatus";
        public static readonly string RefDataAppId = "RefDataAPI:AppId";
        public static readonly string RefDataOcpKey = "RefDataAPI:OcpSubID";
        public static readonly string RefDataGenericUrl = "RefDataAPI:GenericUrl";
        public static readonly string RefDataCacheTimespan = "RefDataAPI:CacheTimeSpan";
        public static readonly string HolidaySearchUrl = "HolidaySearch:Url";
        public static readonly string FlightSearchUrl = "FlightSearch:Url";
        public static readonly string FlightSearchAppId = "FlightSearch:AppId";
        public static readonly string FlightSearchOcpKey = "FlightSearch:OcpSubID";
        public static readonly string RepriceSearchUrl = "Reprice:Url";
        public static readonly string RepriceSearchAppId = "Reprice:AppId";
        public static readonly string RepriceSearchOcpKey = "Reprice:OcpSubID";
        public static readonly string OffersSearchUrl = "Offer:Url";
        public static readonly string OffersSearchAppId = "Offer:AppId";
        public static readonly string OffersSearchOcpKey = "Offer:OcpSubID";
        public static readonly string VHolsQueryAPILogin = "VHolsQueryAPI:Login";
        public static readonly string VHolsQueryAPIRefresh = "VHolsQueryAPI:Refresh";
        public static readonly string VHolsQueryAPIFMB = "VHolsQueryAPI:Forgot";
        public static readonly string VHolsQueryAPIRetrieve = "VHolsQueryAPI:Retrieve";
        public static readonly string OrderSearchUrl = "OrderSearch:Url";
        public static readonly string GetBookingUrl = "GetBooking:Url";
        public static readonly string OrderSearchAppId = "OrderSearch:AppId";
        public static readonly string OrderDataAppId = "OrderData:AppId";
        public static readonly string OrderSearchOcpKey = "OrderSearch:OcpSubID";
        public static readonly string CRMMSUrl = "CRMMS:Url";
        public static readonly string CRMMSAppId = "CRMMS:AppId";
        public static readonly string CRMMSOcpKey = "CRMMS:OcpSubID";
        public static readonly string EnableResponseUrl = "CRMMS:EnableResponseUrl";
        public static readonly string IdentityIssuer = "Identity:Issuer";
        public static readonly string IdentitySignKey = "Identity:SignKey";
        public static readonly string IdentityOcpKey = "Identity:OcpSubID";
        public static readonly string IdentityActivationTokenIssuer = "Identity:IdentityActivationTokenIssuer";
        public static readonly string FlyingClubTokenIssuer = "FlyingClubToken:Issuer";

        public static readonly string CustTransAppId = "CustTrans:AppId";
        public static readonly string CustTransOcpKey = "CustTrans:OcpSubID";
        public static readonly string CustTransactionMsUrl = "CustTrans:MsUrl";

        public static readonly string CampaignManagementAppId = "CampaignManagement:AppId";
        public static readonly string CampaignManagementOcpKey = "CampaignManagement:OcpSubID";
        public static readonly string CampaignManagementMsUrl = "CampaignManagement:CampaignManagementMsUrl";
        public static readonly string AssignCampaignCode = "CampaignManagement:AssignCampaignCode";
        public static readonly string AssignCampaignId = "CampaignManagement:AssignCampaignId";

        public static readonly string ContentServiceUrl = "ContentService:Url";
        public static readonly string ContentServiceOcp = "ContentService:Ocp";

        public static readonly string MemberManagementAppId = "MemberManagement:AppId";
        public static readonly string MemberManagementOcpKey = "MemberManagement:OcpSubID";
        public static readonly string EnrolmentAPIAppId = "Enrolment:AppId";
        public static readonly string BasicEnrolmentMSUrl = "Enrolment:Url";
        public static readonly string EnrolmentMSOcpKey = "Enrolment:OcpKey";
        public static readonly string APIMSubscriptionMSCreateUrl = "APIMSubscriptionMS:CreateUrl";
        public static readonly string APIMSubscriptionMSGetUrl = "APIMSubscriptionMS:GetUrl";
        public static readonly string APIMSubscriptionMSUpdateUrl = "APIMSubscriptionMS:UpdateUrl";
        public static readonly string APIMSubscriptionMSDeleteUrl = "APIMSubscriptionMS:DeleteUrl";
        public static readonly string APIMSubscriptionMSAppId = "APIMSubscriptionMS:AppId";
        public static readonly string APIMSubscriptionMSOcpKey = "APIMSubscriptionMS:OcpKey";
        public static readonly string TimeToWaitBeforeNextRecall = "TimeToWaitBeforeNextRecall";
        public static readonly string IdentityMSAppId = "IdentityMS:AppId";
        public static readonly string IdentityMSOcpKey = "IdentityMS:OcpKey";
        public static readonly string RefDataMSAppId = "RefDataMS:AppId";
        public static readonly string RefDataMSOcpKey = "RefDataMS:OcpSubID";
        public static readonly string RefDataMSGenericUrl = "RefDataMS:GenericUrl";
        public static readonly string RefDataMSCacheTimespan = "RefDataMS:CacheTimeSpan";
        public static readonly string AuditAppId = "Audit:AppId";
        public static readonly string AuditOcpKey = "Audit:OcpSubID";
        public static readonly string CustomerProfileRetrievalUrl = "CustTrans:CustomerProfile";
        public static readonly string MyAccountCustomerMSURLForEmailUpdate = "MyAccount:CustomerMSURLForEmailUpdate";
        public static readonly string MemberManagementHHValidation = "MemberManagement:HouseHoldInviteValidationURL";
        public static readonly string MemberStatusesForAccrual = "ListOfStatusForAccrual";
        public static readonly string VredMsUrl = "VRed:MsUrl";
        public static readonly string CommonCachingSlidingExpiration = "CommonCachingSlidingExpiration";
        public static readonly string CommonCachingAbsoluteExpiration = "CommonCachingAbsoluteExpiration";
        public static readonly string BookVRedVirginPoints = "Book:VRed:VirginPoints";
        public static readonly string BookVRedAppId = "Book:VRed:AppId";
        public static readonly string InRuleAppId = "InRule:AppId";
        public static readonly string InRuleUrl = "InRule:Url";
        public static readonly string AnalyticsMSUrl = "Analytics:Url";
        public static readonly string AanalyticsOcpSubId = "Analytics:OcpSubId";
        public static readonly string CheckInMSUrl = "CheckIn:MsURL";
        public static readonly string CheckInMsOcpKey = "CheckIn:OcpSubID";
        public static readonly string CheckInIdJwtKey = "CheckIn:CheckInIdJwtKey";
        public static readonly string RequirementIdExpiryTimeinMin = "CheckIn:RequirementIdExpiryTimeinMin";
        public static readonly string TravelPartyIdExpiryTimeinMin = "CheckIn:TravelPartyIdExpiryTimeinMin";
        public static readonly string BasketIdExpiryTimeinMin = "CheckIn:BasketIdExpiryTimeinMin";
        public static readonly string PaxOpsJwtTokenExpiryTimeinDays = "CheckIn:PaxOpsJwtTokenExpiryTimeinDays";
        public static readonly string BaggageMSUrl = "Baggage:MsURL";
        public static readonly string BaggageMsOcpKey = "Baggage:OcpSubID";
        public static readonly string BaggageOciTripType = "Baggage:OciTripType";

        /* General */
        public static readonly string PersistedQueryStore = "PersistedQueryStore";
        public static readonly string GraphqlTimeOutInSeconds = "GraphqlTimeOutInSeconds";

        public static readonly string HttpClientTimeBetweenCalls = "HttpClientTimeBetweenCalls";
        public static readonly string HttpClientTimeOutInMilliSeconds = "HttpClientTimeOutInMilliSeconds";
        public static readonly string ReleaseEnvironment = "environment";
        public static readonly string LoggingProvider = "LoggingProvider";
        public static readonly string DatadogLogsEndpoint = "DatadogLogsEndpoint";
        public static readonly string DatadogApiKey = "DatadogApiKey";
        public static readonly string DatadogService = "DatadogService";
        public static readonly string DatadogSource = "DatadogSource";
        public static readonly string DatadogVersion = "DatadogVersion";
        public static readonly string DatadogTags = "DatadogTags";
        public static readonly string DatadogBaggageAllowList = "DatadogBaggageAllowList";
        public static readonly string DatadogMinimumLogLevel = "DatadogMinimumLogLevel";
        public static readonly string DatadogQueueCapacity = "DatadogQueueCapacity";
        public static readonly string DatadogBatchSize = "DatadogBatchSize";
        public static readonly string DatadogFlushIntervalMilliseconds = "DatadogFlushIntervalMilliseconds";
        /// <summary>
        ///  To enable grapghql caching define entry point with a timespan value, this will be used for cache expiry.
        ///  
        ///     e.g.    "Cache:VAA.EAPI.GraphQL.GraphQL.GetSchema": "expiry=00:00:10",
        ///             ...
        ///     
        /// To enable microservice caching define {HttpMethod}:{path/query} with a timespan value, this will be used for cache expiry.
        ///     e.g.
        ///             "Cache:post:https://tempuri.org/api/token": "expiry=00:00:10",
        ///             ...
        /// </summary>
        public static readonly string RedisCacheConnectionString = "RedisCacheConnectionString";

        #region MyAccount - Loyalty Enrolment
        public static readonly string EnableMockResponseForLoyaltySignUp = "MyAccount:EnableMockResponseForLoyaltySignUp";
        public static readonly string LoyaltyEnrolmentMSURL = "MyAccount:EnrolmentSignUp:URL";
        public static readonly string EnrolmentSignUpAppId = "Enrolment:AppId";
        public static readonly string EnrolmentSignUpOCPKey = "Enrolment:OcpKey";
        public static readonly string EnrolmentSubjectValue = "EnrolmentSubject";
        public static readonly string EnrolmentEventTypeValue = "EnrolmentEventType";
        public static readonly string EnrolmentEventSubscriberValue = "EnrolmentEventSubscriber";
        public static readonly string EnrolmentChannelId = "WEB-MYA";
        public static readonly string AllowMemberEnrolToAzureB2C = "MyAccount:AllowMemberToEnrolToAzureB2C";
        public static readonly string IdentityMSURLForVerifyMemberInAzureB2C = "MyAccount:IdentityMSURLForVerifyMemberInAzureB2C";
        public static readonly string MyAccountIdentityMSURLForVerifyEmailtoAzureB2C = "MyAccount:IdentityMSURLForVerifyEmailtoAzureB2C";
        public static readonly string AddressLookUpURLForAddressDetails = "MyAccount:AddressLookUpURLForAddressDetails";
        public static readonly string EnableMockResponseForEnrolment = "MyAccount:EnableMockResponseForEnrolment";
        public static readonly string DeDupFCGenUrl = "MyAccount:DeDupFCGenUrl";
        public static readonly string EnableAzureEndpointAddressDetails = "EnableAzureEndpointAddressDetails";
        public static readonly string AddressLookUpURLForAddressDetailsV2 = "MyAccount:AddressLookUpURLForAddressDetailsV2";
        public static readonly string AddressLookupAPIAppId = "AddressLookup:AppId";
        public static readonly string CustomerMSURLForMemberDetails = "MyAccount:CustomerMSURLForMemberDetails";
        public static readonly string VRedOcpSubID = "VRed:OcpSubID";
        public static readonly string EnableCustomerMs = "EnableCustomerMs";
        public static readonly string EnableMockResponseForPointDotCom = "MyAccount:EnableMockResponseForPointDotCom";
        public static readonly string FutureBookingUrl = "MyAccount:FutureBookingUrl";
        public static readonly string OrderFutureBookingUrl = "OrderDataMs:FutureBookingUrl";
        public static readonly string VRedAppId = "VRed:AppId";
        public static readonly string SecurityAppId = "Security:AppId";
        public static readonly string SecurityMSURLForGetTokenForShopsAway = "MyAccount:SecurityMSURLForGetTokenForShopsAway";
        public static readonly string MyAccountEmailMSURLForPasswordResetConfirmation = "MyAccount:EmailMSURLForPasswordResetConfirmation";
        public static readonly string EmailMSAppId = "Email:AppId";
        public static readonly string MyAccountPasswordResetConfirmationTemplate = "MyAccount:PasswordResetConfirmationTemplate";
        public static readonly string CustomerMSUrlForUpdateMemberAddress = "MyAccount:CustomerMSUrlForUpdateMemberAddress";
        public static readonly string PostCodeMandatoryCountryList = "MyAccount:PostCodeMandatoryCountryList";
        public static readonly string WalletGeneratePassURL = "Myaccount:WalletGeneratePassURL";
        public static readonly string WalletAppId = "Wallet:AppId";
        public static readonly string InRuleOcpSubID = "InRule:OcpSubID";
        public static readonly string IdentityMsUrl = "Identity:MsUrl";
        public static readonly string MobileIosMinimumEnrolmentVersion = "MyAccount:MobileEnrolment:MinimumIosVersion";
        public static readonly string MobileAndroidMinimumEnrolmentVersion = "MyAccount:MobileEnrolment:MinimumAndroidVersion";

        //RetailTherapy
        public static readonly string CreateEarnTransactionUri = "FEAPI:CreateCreditManualAdjustment";
        public static readonly string CreateBurnTransactionUri = "FEAPI:CreateManualRedempton";
        public static readonly string TransactionAppId = "FEAPI:AppId";
        public static readonly string TransactionOcpKey = "FEAPI:OcpKey";
        public static readonly string CancelTransactionUri = "FEAPI:CancelTransaction";
        public static readonly string UpdatePhoneNumber = "ManageAccount:UpdatePhoneNumber";
        public static readonly string MyAccountVRedMSURLForCreateTransaction = "MyAccount:VRedMSURLForCreateTransaction";
        #endregion

        public static readonly string AuditMsUrl = "Audit:MsUrl";
        public static readonly string ReferenceMsUrl = "Reference:MsUrl";
        public static readonly string ReferenceMsOcpKey = "ReferenceMSOcpKey";
        public static readonly string LoyaltyMemberManagementMsUrl = "MemberManagement:MsUrl";
        public static readonly string CustomerMsUrl = "Customer:MsUrl";
        public static readonly string OrderSearchMSUrl = "OrderSearch:MSUrl";
        public static readonly string LoyaltySync = "LoyaltySync:MsUrl";
        public static readonly string LoyaltySyncOcpSubId = "LoyaltySync:OcpSubId";

        public static readonly string AddressSearchbyPostcodeUrl = "AddressSearchbyPostcode:MsUrl";
        public static readonly string AddressSearchAPIAppId = "AddressLookup:AppId";

        public static readonly string BasketMSUrl = "BasketMS:Url";
        public static readonly string BasketMSOcpKey = "BasketMSOcpKey";

        public static readonly string LoyaltySyncAppId = "LoyaltySync:AppId";

        public static readonly string MyAccountInflightConnectivityMSURL = "MyAccount:InflightConnectivityMSURL";
        public static readonly string MyAccountInflightConnectivityExpiry = "MyAccount:InflightConnectivityExpiry";
        public static readonly string MyAccountInflightConnectivityLimitedExpiry = "MyAccount:InflightConnectivityLimitedExpiry";
        public static readonly string MyAccountInflightConnectivityOcpKey = "MyAccount:InflightConnectivityOcpKey";

        #region partner auto-retro       
        public static readonly string SkyMsGenericUrl = "SkyMs:GenericUrl";
        public static readonly string SkyMsAppId = "SkyMs:AppId";
        public static readonly string SkyMSOcpKey = "SkyMs:OcpSubID";
        public static readonly string SkyPartnerRetroCreditFeedbackEnabled = "SkyPartnerRetroCreditFeedbackEnabled";
        #endregion

        //Missing points
        public static readonly string EnableMockResponseForPartnersFlightsPointsClaim = "MyAccount:EnableMockResponseForPartnersFlightsPointsClaim";
        public static readonly string EnableMockResponseForVSPointsClaim = "MyAccount:EnableMockResponseForVSPointsClaim";
        public static readonly string EnableMockResponseForVSRetroClaim = "Loyalty:EnableMockResponseForVSRetroClaim";
        public static readonly string PdmMSUrl = "PdmMSUrl";
        public static readonly string PdmMSAppId = "PdmMSAppId";

        public static readonly string InRuleMSUrl = "InRuleMSUrl";
        public static readonly string InRuleMSAppID = "InRuleMSAppID";
        public static readonly string EnableFlagGetBookingPreValidationResponse = "MyAccount:EnableFlagGetBookingPreValidationResponse";
        public static readonly string RetroFlightActivityNoOfDays = "Loyalty:RetroFlightActivityNoOfDays";

        //VAC
        public static readonly string EnableMockResponseForVACLoginOverview = "EnableMockResponseForVACLoginOverview";
        public static readonly string EnableOrderSearchFlightService = "EnableOrderSearchFlightService";
        public static readonly string EnrolmentTokenFlag = "EnrolmentTokenFlag";
        public static readonly string YoungFlyerAgeLimit = "YoungFlyerAgeLimit";
        public static readonly string EnableUnder18StandardEnrolmentBlock = "EnableUnder18StandardEnrolmentBlock";

        public static readonly string UpdateMemberProfileUrl = "";
        public static readonly string OrderDataMSUrl = "OrderData:MSUrl";
        public static readonly string OrderDataOcpKey = "OrderData:OcpSubID";


        //Stamping
        public static readonly string StampingFCNumberToPNRUrl = "OrderManagement:MsUrl";
        public static readonly string OrderManagementAppId = "OrderManagement:AppId";
        public static readonly string OrderManagementOcpKey = "OrderManagement:OcpKey";

        public static readonly string AccountTripMsUrl = "TRIPMS:MSUrl";
        public static readonly string AccountTripMsOcpKey = "TRIPMS:OcpSubID";
        public static readonly string BookingIdJwtKey = "TRIPMS:BookingIdJwtKey";

        public static readonly string CommonCachingSlidingExpirationInDay = "CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "CommonCachingAbsoluteExpirationInDay";

        public static readonly string FlightServiceMsUrl = "FlightServiceMS:MSUrl";
        public static readonly string FlightServiceMsOcpKey = "FlightServiceMS:OcpSubID";

        public static readonly string SeatMSOcpKey = "BookSeatMSOcpKey";
        public static readonly string SeatMSUrl = "BookSeatMSUrl";
        public static readonly string RetrieveSeatOfferUrl = "BookRetrieveSeatOfferUrl";
        public static readonly string UpdateSeatOfferMSUrl = "BookUpdateSeatOfferMSUrl";

        public static readonly string LOPAMSOcpKey = "LOPAMSOcpKey";
        public static readonly string LOPAMSUrl = "LOPAMSUrl";

        public static readonly string SeatMicroserviceUrl = "Seat:MSUrl";

        public static readonly string ValidateTxnDelayInMS = "ValidateTxnDelayInMS";
        public static readonly string ValidateTxnRetryLimit = "ValidateTxnRetryLimit";
        public static readonly string PaymentMSUrl = "PaymentMS:Url";
        public static readonly string PaymentMSOcpKey = "Book:PaymentMS:OcpSubID";

        public static readonly string FlightOpsUrl = "TripSettings:FlightOpsUrl";
        public static readonly string FlightOpsOcpKey = "TripSettings:FlightOpsOcpSubID";
        public static readonly string EnableSeatDeletion = "TripSettings:EnableSeatDeletion";
        public static readonly string ExcludeGroupBookings = "TripSettings:ExcludeGroupBookings";
        public static readonly string HolidayMaxPaymentAmountInGBP = "TripSettings:HolidayMaxPaymentAmountInGBP";

        public static readonly string TripAircraftDetailsFeature = "TripSettings:AircraftDetailsFeature";

        //Holidays
        public static readonly string StoreAppointmentMsUrl = "StoreLocator:MsUrl";
        public static readonly string StoreAppointmentMsOcpKey = "StoreLocator:OcpSubID";

        // Refunds
        public static readonly string CRMRefundStatusUrl = "CRM:Refunds:Status:Url";
        public static readonly string CRMRefundSubmitUrl = "CRM:Refunds:Submit:Url";
        public static readonly string CRMRefundAppId = "CRM:Refunds:AppId";
        public static readonly string CRMRefundOcpKey = "CRM:Refunds:OcpKey";

        public static readonly string McpCallerContextApplicationId = "Mcp:CallerContext:ApplicationId";
    }
}
