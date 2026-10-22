namespace VAA.EAPI.GraphQL.Common.Constants
{
    public static class Application
    {
        public static readonly string FunctionAppDirPath = ".";
        public static readonly string CommaDelimiter = ",";

        public static readonly string ErrorFilePath = "ErrorCodes\\ErrorCodes.json";

        public static string DeltaGenericErrorCode = "2007";

        public static readonly string ISODateFormat = "yyyy-MM-dd";
        public static readonly string ISODateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        public static readonly string ShortDateTimeFormat = "ddMMMyy";
        public static readonly string TimeFormat = "hhmmt";

        public static string OcpKeyHeader = "Ocp-Apim-Subscription-Key";
        public static string AcceptHeader = "Accept";
        public static string ChannelId = "ChannelId";

        public static readonly string ResponseHeaderKey = "ResponseHeaders";

        public static readonly string DeltaPtcAdult = "ADT";
        public static readonly string DeltaPtcTeen = "GBE";
        public static readonly string DeltaPtcChild = "CNN";
        public static readonly string DeltaPtcInfant = "INF";

        public static string KeyVaultUri = "KeyVaultUri";
        public static string KeyVaultTenantID = "KeyVaultTenantId";
        public static string KeyVaultClientID = "KeyVaultClientId";
        public static string KeyVaultClientSecret = "KeyVaultClientSecret";

        public static readonly string CacheConnectionTimeout = "CacheConnectionTimeout";


        //Servicing
        public static readonly string SelfServiceChannelId = "SELFSERVICE";
        public static readonly string ClaimType = "Claim";


        public static readonly string WebMyaChannelID = "WEB-MYA";
        public static readonly string VerificationTokenNotValidErrorCode = "G001";
        public static readonly string B2COperationFailed = "G046";
        public static readonly string VerificationTokenNotValidErrorMessage = "Provided B2C token has either expired or Invalid. Kindly re-try with correct B2C token.";

        public static readonly string ErrorSource = "Cosuming application";
        public static readonly string ErrorCode = "code";

        public static readonly string InvalidPasswordErrorCode = "10138";
        public static readonly string InvalidPasswordErrorMessage = "Password provided not as per policy";
        public static readonly string InvalidPasswordErrorsource = "Consuming application";


        //OTP generation Response
        public static readonly string OTPGenerationFailResposne = "otp generation failed";
        public static readonly string OTPGenerationSuccessResposne = "otp generated successfully";

        public static readonly string OTPValidationSuccessResponse = "otp validated successfully";
        public static readonly string OTPValidationFailResponse = "otp validation failed";

        public static readonly string Token = "Token";
        public static readonly string IdentityMS = "Identity";
        public static readonly string CustomerMS = "Customer";
        public static readonly string CustomerTransaction = "CustomerTransaction";
        public static readonly string WalletMS = "Wallet";
        public static readonly string OrderSearch = "OrderSearch";
        public static readonly string FcNumber = "FcNumber";
        public static readonly string PhoneNo = "PhoneNo";
        public static readonly string Email = "Email";
        public static readonly string Page = "Page";
        public static readonly string PageSize = "PageSize";


        //MyAccount - Activity
        public static readonly string VredDebitTransactionType = "Debit";
        public static readonly string VredCreditTransactionType = "Credit";
        public static readonly string AccrualTransactionType = "Accrual";
        public static readonly string RedemptionTransactionType = "Redemption";
        public static readonly string CabinBonusType = "CabinBonus";
        public static readonly string BaseMilesType = "BaseMiles";
        public static readonly string TierBonusType = "TierBonus";
        public static readonly string PromotionBonusType = "PromotionBonus";
        public static readonly string TierPointsType = "TierPoints";
        public static readonly string VRedMS = "VRed";
        public static readonly int PageSizeResolver = 1;
        public static readonly string VRED = "VRED";
        public const string RedTier = "RED";
        public const string SilverTier = "SILVER";
        public const string GoldTier = "GOLD";
        public const string RetailTherapyPartnerCode = "DF";
        public const string RetailTherapyProductName = "VAA Duty Free";
        public const string FEAPIDateFormat = "dd-MMM-yyyy";
        public static readonly string BlackListedEmaildErrorCode = "10161b";
        public static readonly string BlackListedEmaildErrorMessage = "Email is blacklisted";
        public static readonly string BlackListedEmaildErrordErrorsource = "Consuming application";
        public const string CustomerID = "CustomerID";
        public const string PrefenceType = "VAAMarketingPermissions";
        public const string PreferredPartnerFFP = "PreferredPartnerFFP";
        public static readonly string MandatoryInputFieldsMissingCode = "15001";
        public static readonly string MandatoryInputFieldsMissingMessage = "One of the mandatory field is missing in the request";
        public static readonly string MandatoryInputFieldsMissingErrorsource = "Consuming application";
        public const string CustomerProfileEndPoint = "{CustomerId}/customerProfile";
        public static readonly string TransactionStatus = "Cancelled";
        public const string GroupDocumentEndPoint = "group/customer/{customerId}";
        public const string CreatePartnermemberMembershipEndPoint = "{customerID}/CreatePartnerMembership";
        public const string UpdatePartnermemberMembershipEndPoint = "{customerID}/UpdatePartnerMembership";
        public const string CenturionDescription = "American Express Centurion";
        public const string CenturionPartnerCode = "ACENT";
        public static readonly string MissingB2CTokenErrorCode = "G064";
        public static readonly string InValidB2CTokenErrorCode = "G001";
        public static readonly string DuplicateEmailErrorCode = "10161a";
        public static readonly string DuplicateEmailErrorMessage = "Email Id already exists in our Records";
        public static readonly string InvalidorMissingEmailIDErrorCode = "G013";
        public static readonly string InvalidorMissingEmailIDErrorMessage = "Email id in the request is Invalid or Missing";
        public static readonly string EmailErrorsource = "Consuming application";
        public static readonly string AccountMismatchErrorCode = "2009";
        public static readonly string MinorProfileLoginErrorCode = "2010";
        public const string CreateHouseHoldInviteDoc = "CreateHouseHoldInvite";
        public const string ValidateHouseholdInvitee = "/HouseHoldAccountValidation";
        public static readonly string AccountMismatchMessage = "Account Mismatch Detected";
        public static readonly string MinorProfileLoginMessage = "Minor Profile Login Detected";
        public const string DeleteHouseHoldInviteDoc = "DeleteHouseHoldInvite";
        public const string GetHouseHoldInviteDocInvitee = "getHouseHoldInvitee/{inviteeMemberNumber}";
        public const string GetHouseHoldInviteDocInviter = "getHouseHoldInviter/{inviterMemberNumber}";
        public const string GetFcMemberDetails = "flyingClubMember/{{customerId}}";
        public const int HouseHoldLeaderCount = 1;
        public const string GetAwardsDetails = "{{customerId}}/awards";
        public const string GetTransactionDetails = "{{customerId}}/transaction";
        public const string GetVoucherDetails = "{{customerId}}/Vouchers";
        public const string GetVredStatements = "/VredStmts/{MemberNumber}";
        public const string GetBannerDetails = "{{customerId}}/BannerDetails";

        public const string VsFlownDetailsEndPoint = "/getVSFlownDetails/{TicketNumber}";
        public const string UpdateTicketStatusEndpoint = "/UpdateTicketStatus";
        public const string GetDistanceEndpoint = "GetSegmentDistance/{Origin}/{Destination}";
        public const string GetBookingDetailsEndpoint = "GetBookingDetails";
        public const string GetBookingDetailsEndpointV2 = "GetBookingDetailsV2";
        public const string GetCountryDetails = "GetCountryDetails";
        public const string GetBookingDetailsValidationEndpoint = "Reference/ValidateBookingClass";
        public const string PointsCalculatorEndpoint = "/AirlinePointsEarnCalculator";
        public const string VirginAccountTierCode = "VAC";
        public const string MembershipChangeEndpoint = "{customerID}/membershipschange";
        public const string VAAccountEnrolmentEndpoint = "/VaEnrolment";
        public const string SignInEndpoint = "/SignIn";
        public const string RefreshTokenEndpoint = "/RefreshToken";
        public const string ResetPasswordStartEndpoint = "/ResetPassword";
        public const string EntraOtpValidationtEndpoint = "/OTP";
        public const string EntraNewPasswordEndpoint = "/SubmitNewPassword";
        public const string OtpTokenValidation = "/OtpTokenValidation";
        public const string ValidatePasswordPolicies = "/ValidatePasswordPolicies";
        public const string OTPGenerate = "/OTPGenerate";
        public const string OTPGenerateContinuationToken = "/OTPGenerateContinuationToken";
        public const string EntraOTPGenerate = "/StartSignUp";
        public const string ValidateOTP = "/ValidateOTP";
        public const string Register = "/Register";
        public const string RegisterYoungFlyer = "/RegisterYoungFlyer";
        public const string CompleteSignUp = "/CompleteSignUp";
        public const string ValidateAuthToken = "/ValidateAuthToken";
        public const string TokenEndpoint = "/token";
        public const string InflightConnectivityEnableWifiEndpoint = "/connectivity/EnableInflightWifi";
        public const string ValidateUniqueIdForPCM = "ValidateUniqueIdForPCM";
        public const string AssignCampaign = "AssignCampaign";
        public const string UpdateFCnumberForPCM = "UpdateFCnumberForPCM";
        //Loyalty
        public const string TxnProcessingStatus = "Processed";
        public const string TxnSubType = "Product";
        public static readonly string MilesType = "Miles";
        public const string MilesQualifyingFlag = "N";
        public const string MilesQualifyingFlagForTierPoints = "Y";
        public const string CreateTransactionEndPoint = "{CustomerId}/transaction";
        public static readonly string PointType = "Points";
        public const string VredTransactionEndPoint = "/VredTransaction/createTransaction";
        public const string VredCancelTransactionEndPoint = "/VredTransaction/cancel";
        public static readonly string PrimaryIndicator = "Y";
        public const string VredCardLinksEndPoint = "/VredCardLinks/{MemberNumber}";
        public const string TxnstatementForVoucherAccrual = "Points credit for reward voucher redemption";
        public const string RetrievePartnerMembershipEndPoint = "{customerID}/PartnerMembership";
        public const string VredMSVirginPointsEndPoint = "/{memberID}/VirginPoints";
        public const string AuditMSAmexCenturionAuditHistoryRetrievalEndPoint = "/{customerId}/AmexCenturionAuditHistory";
        public const string AuditMSAwardAuditHistoryRetrievalEndPoint = "/{customerId}/awardAuditHistory";
        public const string AuditMSCustomerProfileAuditHistoryRetrievalEndPoint = "/{customerId}/customerProfileAuditHistory";
        public const string AuditMSPreferenceAuditHistoryRetrievalEndPoint = "/{customerId}/preferenceHistory";
        public const string AuditMSSupplementarySilverAuditHistoryRetrievalEndPoint = "/{customerId}/SupplementarySilverAuditHistory";

        public const string MemberAccountRetrievalEndPoint = "/getloyaltymember/{custid}";
        public const string SSAMemberAccountRetrievalEndPoint = "/supplementarySilver/accountDetails/{customerId}";
        public const string PartnersRetrievalEndPoint = "/Reference/Partners";
        public const string PromotionEnrolledMemberRetrievalEndPoint = "/PromotionEnrolledMembers/{promotionId}";
        public const string MemberHouseholdRetrievalEndPoint = "/HouseholdDetails/{custid}";
        public const string HouseholdLinkingEndPoint = "/household/LinkAccount";
        public const string HouseholdDelinkingEndPoint = "/household/DelinkAccount";
        public const string SupplementarySilverDelinkingEndPoint = "/SupplementarySilver/DelinkAccount";
        public const string SupplementarySilverlinkingEndPoint = "/supplementarySilver/linkAccount";
        public const string AddressSearchbyPostcodeEndPoint = "/FindAddress/{addressText}/{CountryCode}";
        public const string AddressSearchbyRetriveEndPoint = "/Addresses/{addressId}";

        public const string CustomerByCustomerIdEndPoint = "/customer/{customerid}";
        public const string CustomerByMemberIdEndPoint = "/customer/loyalty/{memberid}";
        public const string CustomerBasicEnrolmentEndPoint = "/BasicEnrolment";
        public const string CustomerContactHistoryRetrievalEndPoint = "/{customerId}/contactHistory";
        public const string CustomerAddressHistoryRetrievalEndPoint = "/{customerId}/addressHistory";
        public const string CustomerDocByCustomerIdEndPoint = "/customerdocument/{customerId}";
        public const string CustomerValidateMemberStatusEndPoint = "/ValidateMemberStatus";
        public const string CustomerSearchByphoneEndPoint = "/customerSearchByPhone/{phoneNumber}";

        public static readonly string FCNumberUnavailableErrorCode = "G007";
        public const string OrderSearchMSRetrievePNREndPoint = "order/{PNR}";
        public static readonly string PendingInviteListMemberErrorMessage = "Invite has already been sent to this FC Number";
        public static readonly string TotalHouseHoldAccountErroMessage = "HHA has reached max number of members";
        public static readonly string InviteLinkedMemberErrorMessage = "Inviter is a linked member, can not invite members to HHA";
        public const string RetrieveRewardBookingPointsEndPoint = "/RewardBookingPointCalculation";

        public const string GenerateStatementTextEndPoint = "/transaction/GenerateStatementText";
        public const string SyncSFPDEndPoint = "SyncSFPD";
        public const string AirlinePointsEarnCalculatorEndPoint = "/AirlinePointsEarnCalculator";
        public const string UpdateFCEarnPointsEndPoint = "/updatefcearnpoints";
        public const string SaveInsuranceQuoteEndPoint = "/insurancequote";
        public const string UpdateInsuranceDecisionEndPoint = "/insurancedecision";
        public const string SetCobrandProgressUrl = "/cobrandprogress";

        public const string BulkMemberUpdateEndpoint = "BulkMemberUpdate";

        //Retail Therapy

        public const string ActivitySubtype = "Product";
        public const string PAUser = "PAUSER";
        public const string CreatedUser = "RetailTherapy";
        public static readonly string CustomerMsUrl = "Customer:MsUrl";
        public const string PartnerAutoRetroFFPCarrierCode = "VS";
        public const string PostingStatusFlag = "Y";
        public const string RetroFlag = "Y";
        public const string RetroOverrideFlag = "N";
        public const string SkyMsCreateAutoRetroUrl = "CheckClaimEligibility";
        public const string SkyMsFeedbackRequestUrl = "OnlineRetroCreditFeedback";
        public const string RefMsGetSegmentDistanceUrl = "GetSegmentDistance/";
        public const string RefMsGetSegmentDistanceAndCountryUrl = "GetSegmentDistanceAndCountry/";
        public const string RefMsGetCabinClassDetailsUrl = "GetCabinClassForNonVSRetro";
        public const string VAPostingTypeCode = "02";
        public const string VredCreateTransactionEndpoint = "VredTransaction/createTransaction";
        public const string VredCreateTransactionResponseEndpoint = "createTransaction/response/{correlationid}";
        public const string VSOperatingAirlineCode = "VS";
        public const string TransactionChannel = "WEBSITE";
        public const string PartnerName = "Virgin Atlantic Airways";
        public const string ProductCategory = "Flight";
        public const string GetTransactionEndPoint = "{CustomerId}/transaction";
        public const string KLOperatingAirlineCode = "KL";
        public const string AFOperatingAirlineCode = "AF";

        //Member Merge
        public const string UpdateFlyingClubMemberEndPoint = "/{customerID}/flyingClubMember";
        public const string VredPointsTransferProductName = "Points Merge";
        public const string ContactNoteGroup = "GEN";
        public const string ContactNoteType = "GEN";
        public const string TierPointsTransferProductName = "Tier Points Merge";
        public const string MergedFCMDocStatus = "Merged";
        public const string AwardDocStatusCancelled = "Cancelled";
        public const string ContactNoteText = "Flying Club account {0} is merged with this account";
        public const string VSFlownMiles = "VSFlownMiles";
        public const int TierPointsMonthsValidity = -12;
        public const string VSTierPoints = "VSTierPoints";

        //VAC
        public const string UpdateMemberProfileEndPoint = "";
        public const string VACMemberUpdateEndPoint = "/UpdateVirginAccountCustomer";
        public const string CheckTierDetailswithEmail = "/customerSearchByEmail/{emailId}";
        public const string UpgradeFlyingClubEndpoint = "/UpgradeToFlyingClub";
        public const string MediaCode = "WEB";
        public const string VACMemberPreferenceUpdateEndPoint = "/UpdateMarketingPreferences";
        public const string ProfileUpdateMSSuccess = "Successfully updated";
        public const string ProfileUpdateGraphQlSuccess = "Member Profile Details Updated Successfully";
        public const string RefMsGetCountryDetailsUrl = "/GetCountryDetails";
        public const string BookingsEndpoint = "/FutureBookings";
        public const string UpdatePnrEndpoint = "AddFFP/StampFCNumberToPNR";

        //Wallet
        public const string GeneratePassUrl = "generatePass";

        //Banner
        public const string GetbannerDetails = "/BannerDetails";
        public const string CreatebannerDetails = "CreateBanner";
        public const string UpdatebannerDetails = "/UpdateBanner";

        public const string TrackingAnalyticsType = "GraphQL";
        public const string GetRelatedTransactions = "{customerID}/transaction/cancel/RealtedTxns/{transactionId}";

        public const string CancelTransactionEndPoint = "{customerID}/transaction/cancel";
        public const string VredCancelltionEndPoint = "/VredTransaction/Cancel";

        //OCI 
        public const string GetCheckInDetails = "/CheckIn";
        public const string LogTransactionSummary = "/CheckIn/TransactionSummary";
        public const string SelectPassengers = "/CheckIn/selectPassengers";
        public const string SavePassport = "/CheckIn/savePassport";
        public const string SaveContactTracing = "/CheckIn/SaveContactTracing";
        public const string SaveEmergencyContact = "/CheckIn/SaveEmergencyContactDetails";
        public const string GenderDetails = "/GenderDetails";
        public const string StatesOfUSADetails = "/StatesOfUSADetails";
        public const string CountryProfile = "/Countries";
        public const string NationalityDetails = "/NationalityDetails";
        public const string PassportCountryDetails = "/PassportCountryDetails";
        public const string SaveReturnDate = "/CheckIn/SaveReturnDate";
        public const string SpecialMealCategory = "SPM";
        public const string SpecialAssistanceCategory = "DEA,BLN,WCH";
        public const string StandardMealCategoryChoice = "STND";
        public const string NoAssitAccessibleTravelCode = "NASS";
        public const string NoAssitAccessibleTravelTitle = "No assistance required";
        public const string StandardMealCategoryTitle = "Standard Meal";
        public const string WheelChairAssistCateogyCode = "WCH";
        public const string BlindAssistCategoryCode = "BLN";
        public const string DeafAssistCategoryCode = "DEA";
        public const string BoardingPassDetails = "/CheckIn/BoardingPass";
        public const string GetBoardingPassDetails = "/CheckIn/GetBoardingPasses";
        public const string RetrieveBoardingPass = "/CheckIn/RetrieveBoardingPass";
        public const string PaymentEligibilityCheck = "/CheckPayment";
        public const string BaggageProductType = "BAGGAGE";
        public const string MissingFieldMessage = "Mandatory fields missing in the request: {0}";
        public const string OnlieCheckInFailDefaultMessage = "Looks like something needs sorting at the airport. Our team will take care of it when you check in.";
        public const string OnlieCheckInFailDefaultTiTle = "We can't complete check in online";
        public const string BaggageCheckInFailDefaultMessage = "We’re unable to complete your baggage process online. Our team at the airport will take care of it.";
        public const string BaggageCheckInFailDefaultTiTle = "We can't complete baggage check in online";
        public const string BoardingPassFailDefaultMessage = "Sorry, we were unable to retrieve your boarding pass. Please try again later or our team at the airport can print one for you.";
        public const string CurrencyNumber = "/Air4CurrencyNumberDetails";
        public const string MobilePaymentChannel = "DFMOBILE";
        public const string BaggageProductCode = "CBAG";
        public const string BaggageProductDescription = "service.CBAG";
        public const string BaggageProductCategory = "SERVICE";
        public const string BaggageOfferId = "BaggageOfferId is required and must not be null or empty";
        public const string BaggageTotalPrice = "TotalPrice is required and must be zero or greater.";
        public const string BaggageProductItem = "Excess Baggage";
        public const string BagsAppId = "D2";
        public const string BagsChannelId = "DFMOBILE";
        public const string BagsPurchasePath = "DFMOBILEOCI";
        public const string VSLoyaltyProgrammeCode = "VS";
        //Baggage MS
        public const string GetBaggage = "/GetBaggage";
        public const string SaveBaggage = "/SaveBaggage";
        public const string ConfirmBaggage = "/ConfirmBaggage";

        //Payment MS
        public const string PaymentOrderReferenceIdMSUrl = "/paymentReferenceId";
        public const string PaymentDetailsMSUrl = "/paymentDetails/{paymentReferenceId}";
        public const string HolidayPaymentMSUrl = "/holiday/payment";
        public const string PaymentTypeHeader = "formOfPayment";
        public const string GetMemberCustomAttributes = "{customerID}/memberCustomAttributes";

        //Holidays- Appointment Booker MS
        public const string GetNearByStoresDetails = "/Stores/GetAllStores";
        public const string GetStoresByID = "/Stores/GetStoresById";
        public const string GetSlotsbyDate = "/Stores/GetSlotsByDate";
        public const string SaveAppointmentBooking = "/Holidays/Storeappointments";
        public const string GetCalendarAvailability = "/Stores/GetCalendarAvailability";
        //Partner Data Management MS

        public const string GetFlownDataEndpoint = "/getVSRetroFlownDetails";
        public const string UpdateVSRetroFlownDetailsEndpoint = "/updateVSRetroFlownDetails";
        public const string ValidateVSClaimInputsEndpoint = "/ValidateVSClaimInputs";
    }
}
