namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
{
    public class Application
    {
        public static string CommonSQLRefDBMIToken;
        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public static readonly string EventTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        public static readonly string HeaaderParameterlocation = "location";
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static readonly string EventSpecversion = "1.0";
        public static readonly int UpdateResultNumber = 10;
        public static readonly string Faliure = "Failure";
        public static readonly string Success = "Success";
        public static readonly string GetMemberAccountReferenceDatas = "EXEC [dbo].[SP_GetMemberAccountReferenceData]";
        public static readonly string SpecVersion = "1.0";
        public static readonly string HeaderParameterlocation = "location";
        public static string LoyaltyMbrMgmtClientSecretCached;
        public static string CustomerTxnMSResourceIdCached;
        public static string LoyaltyMbrMgmtOcpKeyCached;
        public static readonly string MemberAccountReferenceDatasCacheKey = "ReferenceData_MemberAccountReferenceData_CACHE_KEY";
        public const string Transaction = "transaction";
        public const string FlyingClubMember = "flyingClubMember";
        public const string Membershipschange = "membershipschange";
        public const string Award = "awards";
        public const string VSFlownMiles = "VSFlownMiles";
        public const string ActiveFlag = "Y";
        public const string TierPoints = "TierPoints";
        public const string VSTierPoints = "VSTierPoints";
        public const string StatusCode = "Processed";
        public const string TransactionType = "Accrual";
        public const string AwardType = "Top Member";

        public static readonly string DummyEmailDomain = "DummyEmailDomain";
        public static readonly string ManageaccountUpdatePhoneSubscriberEventSubject = "PhoneUpdateHH";

        public static readonly string PartnersCacheKey = "ReferenceData_PARTNER_CACHE_KEY";
        public static readonly string GetPartnersData = "EXEC [dbo].[SP_GetAllPartnerDetails]";

        public static readonly string GetMembersForBirthdayEmail = "EXEC [dbo].[SP_MEMBER_BIRTHDAY_DETAILS]";
        public static readonly string GetMembersForBirthdayBonus = "EXEC [dbo].[SP_GET_MEMBERS_FOR_BIRTHDAY_BONUS] @BirthdayDateForRun";
        public static readonly string StageGoldMemberDetailsForWingsAssessment = "EXEC [dbo].[SP_GET_MEMBER_FOR_WINGS_ASSESSMENT] @ProductDescription, @IsSuccessful OUTPUT, @StatusMessage OUTPUT, @RowsInserted OUTPUT";
        public static readonly string GetWingsMemberAssessmentBatch = "EXEC [dbo].[SP_GET_WINGS_MEMBERS_BATCH] @memCount, @HasMoreUnprocessedRecords OUTPUT";
        public const decimal TwoMillionMilerFlownMilesThreshold = 2000000;
        public static readonly string BirthdayEmailProcessSuccessMessage = "Birthday Email Process completed successfully";
        public const string BirthdayBonusCombinedRuleSetEvent = "VAA.CustomerTxns.RuleEngine.CombinedRuleSetEvent";
        public static readonly string GetProductReferenceData = "Select top 1 rowid ProductId, productname ProductName from product where active=1 and productname=";
        public static readonly string BirthdayBonusProcessSuccessMessage = "Birthday Bonus Process completed successfully";
        public const string BirthdayBonusTier = "Gold";
        public const string ActiveStatus = "Active";
        public static readonly string EnrolledMemberWorksheetName = "EnrolledMember";
        public static readonly string EnrolledMemberWorksheetColumnName = "MemberNumber";
        public static readonly string GetPromotionEnrolledMembers = "exec [dbo].[SP_PROMOTION_EnrolledMembers] @PromotionId";
        public static readonly string MembersNotFound = "No Associated Members with given Promotion Id";

        public const string TargetedMembershipEventTopicName = "loyaltymembermanagementeventmessages";
        public const string TargetedMembershipDeleteEventSubscriptionName = "DeleteTargetedMembershipMessageSubscriber";
        public static readonly string DeleteTargetedMembership = "EXEC [dbo].[SP_DELETE_CAMPAIGN_MEMBERS] @campaignId, @customerId, @Result OUTPUT";
        public const string TargetedMembershipNotFound = "TargetedMembership Data Not Found";
        public static readonly string DeleteSuccessMessage = "Record is successfully deleted from Reporting DB";

        public static readonly string FCMemberActiveStatus = "Active";
        public static readonly string FCMemberFrozenStatus = "Frozen";
        public static readonly string FCMemberActivePendingStatus = "Active Pending";
        public static readonly string FCMemberActivePendStatus = "ActivePending";
        public static readonly string FCMemberExpiredStatus = "Expired";
        public static readonly string FCMemberInactiveStatus = "Inactive";

        public static readonly string Brand = "VAA";
        public static readonly string SchemaVersion = "0.1";
        public static readonly string HouseholdGroupTypeCode = "Household";
        public static readonly string HoHLinkingInitiatedMessage = "Request successfully initiated";
        public static readonly string FCMemberSilver = "Silver";
        public static readonly string FCMemberGold = "Gold";
        public static readonly string FCMemberRed = "Red";
        public static readonly string FCMemberTypeIndividual = "Individual";
        public static readonly string FCMemberTypeLinked = "Linked";

        public static readonly string ChildRelationshipType = "Linked Member";
        public static readonly string HoHManagedChildRelationshipType = "Linked Member-Child";
        public static readonly string ParentRelationshipType = "Head of Household";
        public const string YoungFlyerParentEmailEvent = "InviteEmailEvent";
        public const string YoungFlyerParentEmailEventTopicName = "householdaccount";
        public static readonly string FetchGroupDocumentsByHeadCustomerId = "group/customer";
        public static readonly string FetchGroupDocumentsFromGroupHistoryCustomerId = "groupHistory";
        public const string DelinkAccountMemberStatusUpdateEventTopicName = "loyaltymembermanagementeventmessages";
        public const string DelinkAccountMemberStatusUpdateEventSubscriptionName = "DelinkAccountMemberStatusUpdateMessageSubscriber";
        public const string MemberStatusEventSubscriptionName = "memberstatussubscriber";
        public const string MemberTierUpdateEventSubscriptionName = "MemberTierUpdateEventSubscriber";

        public static string CustomerMSResourceIdCached;
        public const string HouseholdDissolveEventSubscriber = "HouseholdDissolveEventSubscriber";
        public static readonly string GetTransactionProductReferenceData = "Select top 1 rowid ProductId, productname ProductName from product where productname='Million Miler' and active=1";
        public const string MillionMilerEmailEventSubscriptionName = "MillionMilerEmailEventSubscriber";
        public const string PartnerCode = "VS";
        public const string MillionMilerUser = "Loyalty Member Management";
        public static readonly string TxnDateTimeFormat = "yyyy-MM-dd hh:mm:ss";
        public const string AccrualTrxnType = "Accrual";
        public const string AccrualTrxnSubType = "Product";
        public const string MilesPointType = "Miles";
        public const string BaseMilesPointSubType = "BaseMiles";
        public const string TransactionValue = "100000";
        public const string MillionMilerTrxnChannel = "Auto Created";
        public const string SuccessTrxnStatus = "Processed";
        public const string StatementText = "Congratulations! We have added 100,000 Virgin Points to your account for flying 1 million miles with us";
        public const string MilesQualifyingFlag = "N";
        public const string ServiceBusTopicName = "loyaltymembermanagementeventmessages";
        public const string MillionMilerAwardEventSubscriptionName = "MillionMilerAwardEventSubscriber";
        public static readonly string SuccessMessage = "Transaction successfully created";

        public const string LTGAwardEventSubscriber = "LTGAwardEventSubscriber";
        public const string MillionMilerProductName = "Million Miler Bonus";

        public static readonly string SSAMemberRelationShipTypeCodeSupplementarySilver = "Supp Silver";
        public static readonly string SSAMemberRelationShipTypeCodeLinkedGold = "Linked Gold";
        public static readonly string FetchFCMDocBySSAMemberId = "flyingClubMember/supplementarySilverAccount";
        public static readonly string RequestSuccessfullyInitiatedMessage = "Request successfully initiated";

        public const string AddressUpdateNotificationEventSubscriber = "AddressUpdateNotificationEventSubscriber";
        public static readonly string AddressUpdateNotificationSuccess = "Address change event is published successfully";
        public const string AddressChangeEventSubscriber = "AddressChangeEventSubscriber";
        public const string EmailChangeEventSubscriber = "EmailChangeEventSubscriber";
        public const string UpdatePhoneNumberMessageSubscriber = "UpdatePhoneNumberMessageSubscriber";
        public static readonly string AddressChangeEventSuccess = "Address Change is initiated successfully";
        public const string CustomerDocument = "customerdocument";
        public const string CustomerSearchDocument = "customersearch";
        public const string CustomerServiceBusTopicName = "customereventmessages";

        public const string LoyaltyMemberStatusActive = "Active";
        public const string LoyaltyMemberStatusFrozen = "Frozen";
        public const string LoyaltyMemberStatusSuspended = "Suspended";

        public static readonly string EventInitiatedMessage = "Cloud Event  successfully initiated";
        public const int ParamCount = 1;
        public const string AddSSRPassengersFlag = "N";
        public const string AddSSRSegmentsFlag = "Y";
        public const string AddSSRProductType = "SSR";
        public const string AddSSRRequestCode = "FQTV";
        public const string AddSSRStatusCode = "SS";
        public const string AddSSRSubject = "ManageSpecialService";
        public const string MandatoryInputMissing = "MandatoryInputMissing";

        public const string LoyaltyGenericAuditSSAAuditCategory = "Supplementary Silver";
        public const string LoyaltyGenericAuditSSAAuditSubCategoryLinking = "Linking";
        public const string LoyaltyGenericAuditSSAAuditSubCategoryDelinking = "Delinking";
        public const string LoyaltyGenericAuditSSAAuditRelationshipTypeLinkedGold = "Linked Gold";
        public const string LoyaltyGenericAuditSSAAuditRelationshipTypeSuppSilver = "Supp Silver";
        public const string LoyaltyGenericAuditDefaultSourceIdentiferLoopsAPI = "LOOPS API";
        public const string FCNumberToPNRResponseEventSubscriber = "FCNumberToPNRResponseEventSubscriber";

        //Pos hardcoded to LON to fix other GDS bookings as PNR session creation fails for those locations(eg SWI) therefore not using Pos input from source file
        public const string SeatUptakePos = "LON";

        public const string CountryUK = "United Kingdom";
        public const string CountryUS = "United States Of America";
        public const string InfantPTC = "INF";
        public const string ManageAccountTopicName = "manageaccount";
        public static readonly string DataVersion = "0.1";

        //BulkMemberUpdate
        public static readonly string UpdateCustomerEventSubjectinVRED = "UpdateVredEvent";
        public static readonly string UpdateCustomerEventSubjectinB2C = "UpdateAzureB2CProfile";
        public static readonly string UpdateCustomerEventSubjectinAudit = "VAA.Loyalty.Audit.CustomerProfileEvent";
        public static readonly string UpdateCustomerEventSubjectinCustomer = "VAA.Loyalty.Customer.UpdateCustomerDocument";
        public static readonly string UpdateCustomerEventSubjectinCustomerTrans = "ContactNoteCreateEvent";
        public static readonly string CustomerTransactioneventmessagesType = "VAA.PowerApp.Customer.CreateContactNote";
        public static readonly string CustomerEventmessagesTopicName = "customereventmessages";
        public static readonly string CustomerTransactioneventmessagesSubject = "ContactNotes";
        public static readonly string CustomerTransactionStatuseventmessagesSubject = "VAA.PowerApp.Customer.UpdateFlyingClubMember";
        public static readonly string ContactNoteGroup = "GEN";
        public static readonly string ChannelValue = "powerapps";
        public static readonly string CustomerTransactionEventmessagesTopicName = "customertransactioneventmessages";
        public static readonly string AuditEventmessagesTopicName = "auditeventmessages";
        public static string VredMSResourceIdCached;
        public static string VRedMSTokenCached;
        public static readonly string GrantType = "grant_type";
        public static readonly string ClientId = "client_id";
        public static readonly string ClientSecret = "client_secret";
        public static readonly string Resource = "resource";
        public const string ClientCredentials = "client_credentials";
        public static readonly string AuthTokenValue = "oauth2/token";

        //CustomerProfile
        public static readonly string DocType = "CustomerProfile";

        //Tier Points Logic Enhancement
        public static readonly string[] PartnerCodes = { "VS", "VAH", "SYF", "VHOL" };
        public const string FlightProductName = "Flight";

        // NLA Inbound Auto Accrual
        public const string MUAirlineCode = "MU";
        public const string FMAirlineCode = "FM";
        public const string AutoAccrualInvalidAirlineCode = "003.005";
        public const string AutoAccrualInvalidSurname = "003.006";
        public const string AutoAccrualInvalidGivenName = "003.007";
        public const string AutoAccrualInvalidScheduledDepartureDate = "003.014";
        public const string AutoAccrualFfpAccountNotFound = "050.000";
        public const string AutoAccrualFfpAccountSuspended = "050.001";
        public const string AutoAccrualFfpAccountClosed = "050.002";
        public const string AutoAccrualFfpAccountMerged = "050.003";
        public const string AutoAccrualInboundSubscriptionName = "SkyTeamAutoAccrualInboundSubscriber";
        public const string AutoAccrualInboundMessageLabel = "VAA.Loyalty.SkyTeam.InboundAutoAccrualRequest";
        public const string AutoAccrualActivityDateFormat = "yyyy-MM-dd";
        public static readonly string[] MemberStatusesForAccrual = { "Active", "Frozen", "ActivePending", "Expired", "Inactive" };
        public const string GetIneligibleClassValidity = "EXEC [STG].[GetIneligibleClassValidity] @ORIGINAIRPORTCODE, @DESTINATIONAIRPORTCODE, @ACTIVITYDATE, @CREDITCLASS, @PARTNERCODE, @OPERATINGAIRLINECODE, @MKTAIRLINECODE";
        public const string GetNonAccrualClassValidity = "EXEC [STG].[GetNonAccrualClassValidity] @ORIGINAIRPORTCODE, @DESTINATIONAIRPORTCODE, @ACTIVITYDATE, @CREDITCLASS, @PARTNERCODE, @OPERATINGAIRLINECODE, @MKTAIRLINECODE";
        public const string GetMktClassValidity = "EXEC [STG].[GetMKTClassValidity] @ACTIVITYDATE, @OPERATINGAIRLINECODE, @MKTAIRLINECODE, @CREDITCLASS, @PARTNERCODE";
        public const string GetAutoAccrualPartnerTerms = "SELECT TOP 1 pt.AgreementStartDate, pt.AgreementEndDate, pt.RetroClaimPeriod FROM dbo.PartnerProfile pp INNER JOIN dbo.PartnerTerms pt ON pt.PartnerId = pp.RowID WHERE pp.PartnerCode = @PARTNERCODE";
        public const string GetAirlineActivityDateValidity = "EXEC [STG].[GetAirlineActivityDateValidity] @ACTIVITYDATE, @BOOKINGCLASS, @PARTNERCODE";
        public const string PdmEventTopicName = "pdmeventmessages";
        public const string AutoAccrualFeedbackEventName = "VAA.Loyalty.SkyTeam.InboundAutoAccrualFeedbackProcessing";
        public const string AutoAccrualFeedbackDataContentType = "application/json";
        public const string AutoAccrualFeedbackSuccessStatus = "Success";
        public const string AutoAccrualFeedbackErrorStatus = "Error";
        public const int AutoAccrualFeedbackPlaceholderMiles = 0;

        public static readonly string FlightProductCacheKey = "ReferenceData_Product_Flight_CACHE_KEY";
        public const string CustomerTransactionStatusEndpoint = "transactions/status";
        public const string AutoAccrualUser = "SkyTeam Auto Accrual";
        public const string AutoAccrualTransactionChannel = "PDM";
        public const string CabinBonusPointSubType = "CabinBonus";
        public const string TierBonusPointSubType = "TierBonus";
        public const string PromotionBonusPointSubType = "PromotionBonus";
        public const string PointsPointType = "Points";
        public const string TierPointsPointType = "TierPoints";
        public const string TierPointsQualifyingFlag = "Y";
        public const string RuleEngineAirlinePointsEarnCalculatorEndpoint = "/AirlinePointsEarnCalculator";
        public const string RuleEngineWingsMemberAssessmentEndpoint = "/ExecuteTierRuleSet";
        public const string GetDistanceOfRouteForInRuleProcedure = "[STG].[GetDistanceOfRouteForInRule]";

    }
}
