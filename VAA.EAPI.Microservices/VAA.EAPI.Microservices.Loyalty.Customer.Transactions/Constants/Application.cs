namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants
{
    public class Application
    {
        // Enviromental settings
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        // Contact Notes
        public static readonly string ContactNoteBrand = "VAA";
        public static readonly string ContactNoteDocSchemaVersion = "0.1";

        public const string ContactNoteEventTopicName = "customertransactioneventmessages";
        public const string ContactNoteCreateEventSubscriptionName = "CreateContactNotesMessageSubscriber";
        public const string ContactNoteDeleteEventSubscriptionName = "DeleteContactNotesMessageSubscriber";
        public const string ContactNoteUpdateEventSubscriptionName = "UpdateContactNotesMessageSubscriber";

        // Targeted Membership
        public static readonly string TargetedMembershipBrand = "VAA";
        public static readonly string TargetedMembershipDocSchemaVersion = "0.1";

        // Transaction Document
        public static readonly string TransactionBrand = "VAA";
        public static readonly string TransactionDocSchemaVersion = "0.1";
        public const string TransactionEventTopicName = "customertransactioneventmessages";
        public const string TransactionCreateEventSubscriptionName = "CreateTransactionMessageSubscriber";
        public const string TransactionCancelEventSubscriptionName = "CancelTransactionMessageSubscriber";
        public const string TransactionUpdateEventSubscriptionName = "UpdateTransactionMessageSubscriber";

        // Invite House Hold 
        public const string ActiveStatus = "Active";
        public static readonly string HouseholdGroupTypeCode = "InviteHouseHold";
        public static readonly string PendingInvite = "VAA.MyAccount.Customer.CreatePendingInviteDoc";
        public const string CreatePendingHouseHoldMessageSubscriber = "CreatePendingHouseHoldMessageSubscriber";
        public static readonly string PendingInviteEmail = "InviteEmailEvent";
        public const string InviteHouseHoldEmailEventTopicName = "householdaccount";
        public static readonly string InviteHouseHoldDbQueryforInvitee = "SELECT * FROM c WHERE c.PartitionKey = @PartitionKey  and c.InviteeMemberNumber = @InviteeMemberNumber";
        public static readonly string InviteeMemberNumber = "InviteeMemberNumber";
        public static readonly string InviteHouseHoldDocType = "InviteHouseHold";
        public const string DeletePendingHouseHoldMessageSubscriber = "DeletePendingHouseHoldMessageSubscriber";
        public static readonly string DeleteInvite = "VAA.MyAccount.Customer.DeletePendingInviteDoc";
        public static readonly string InviterHouseHoldDbQuery = "SELECT * FROM c WHERE c.PartitionKey = @PartitionKey  and c.InviterMemberNumber = @InviterMemberNumber";
        public static readonly string InviterMemberNumber = "InviterMemberNumber";

        //Banner
        public static readonly string BannerDetailsDbQuery = "SELECT * FROM c WHERE c.PartitionKey = @PartitionKey  and c.DocType = @DocType";
        public static readonly string BannerDocType = "Banner";

        public const string TargetedMembershipEventTopicName = "customertransactioneventmessages";
        public const string TargetedMembershipCreateEventSubscriptionName = "CreateTargetedMembershipMessageSubscriber";
        public const string TargetedMembershipDeleteEventSubscriptionName = "DeleteTargetedMembershipMessageSubscriber";
        public const string TargetedMembershipUpdateEventSubscriptionName = "UpdateTargetedMembershipMessageSubscriber";

        // EnrolmentDoc Membership
        public const string CampaignEnrolmentDocEventTopicName = "customertransactioneventmessages";
        public const string CampaignEnrolmentDocEventSubscriptionName = "CampaignEnrolmentDocMessageSubscriber";

        //Inrule
        public const string CreateMembershipChangeEventSubscriptionName = "CreateMembershipChangeMessageSubscriber";
        public const string UpdateMembershipChangeEventSubscriptionName = "UpdateMembershipChangeMessageSubscriber";

        // Awards
        public static readonly string AwardBrand = "VAA";
        public static readonly string AwardDocSchemaVersion = "0.1";

        public const string AwardEventTopicName = "customertransactioneventmessages";
        public const string AwardCreateEventSubscriptionName = "CreateAwardMessageSubscriber";
        public const string AwardDeleteEventSubscriptionName = "DeleteAwardMessageSubscriber";
        public const string AwardUpdateEventSubscriptionName = "UpdateAwardMessageSubscriber";
        public const string AssignTopMemberEventSubscriptionName = "AssignTopMemberEventSubscriber";

        public const string WingsMemProductID = "1634561747";
        public const string LTGProductID = "1634561772";
        public const string MillionMilerProductID = "1634561592";
        public const int TwoMillionMilerFlownMiles = 2000000;

        //Activity
        public static readonly string ActivityMemberBrand = "VAA";
        public static readonly string ActivityDocSchemaVersion = "0.1";

        // Flying Club Member
        public static readonly string FlyingClubMemberBrand = "VAA";
        public static readonly string FlyingClubMemberDocSchemaVersion = "0.1";
        public static readonly string FlyingClubMemberTypeIndividual = "Individual";
        public static readonly string SSAMemberNumberFieldName = "SSAMemberNumber";
        public static readonly string FCMDocBySSAMemberNumberSQL = "SELECT TOP 1 * FROM c WHERE c.DocType = @DocType and c.SSAMemberNumber = @SSAMemberNumber Order By c._ts DESC";
        public static readonly string VSFlownMiles = "VSFlownMiles";

        public const string FlyingClubMemberEventTopicName = "customertransactioneventmessages";
        public const string FlyingClubMemberUpdateEventSubscriptionName = "UpdateFlyingClubMemberMessageSubscriber";

        // Transaction DB
        public static readonly string CustomerTransactionsSQL = "SELECT TOP 1000 {fieldlist} FROM c WHERE c.DocType = @DocType and c.PartitionKey = @PartitionKey Order By c.LastUpdatedOn DESC";
        public static readonly string DocTypeFieldName = "DocType";
        public static readonly string PartitionKeyFieldName = "PartitionKey";
        public static readonly string AccrualTransactionType = "Accrual";

        // Service Bus
        public static readonly string SpecVersion = "1.0";

        // Command Response 
        public static readonly string HeaderParameterlocation = "location";
        public static readonly string CommandResponseStatusKey = "CustomerTransactions";

        //TierRuleSet Event
        public const string UpdateFCMEventSubject = "VAA.PowerApp.Customer.UpdateFlyingClubMember";
        public const string TierRuleSetEvent = "VAA.CustomerTxns.RuleEngine.TierRuleSetEvent";
        public const string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public const string DBTimeFormat = "yyyy-MM-dd hh:mm:ss";
        public const string S2GFlag = "S2GFLAG";
        public const string S2SFTGFlag = "S2SFTGFLAG";
        public const string S2SFlag = "S2SFLAG";
        public const string FirstVSFlightActivity = "FIRSTVSFLIGHTACTIVITY";
        public const string RetainReason = "TRN";
        public const string DowngradeReason = "TDG";
        public const string UpgradeReason = "TUG";
        public const string RedTier = "Red";
        public const string SilverTier = "Silver";
        public const string GoldTier = "Gold";
        public const string FirstVSFlightActivityDefaultValue = "N";

        //GroupDoc
        public const string GroupDocUpdateEventSubscriptionName = "UpdateGroupDocumentMessageSubscriber";
        public const string GroupDocCreateEventSubscriptionName = "CreateGroupDocumentMessageSubscriber";
        public const int MaxGroupHistoryCount = 10;
        public static readonly string HeadOfGroupCustomerIdFieldName = "HeadOfGroupCustomerId";
        public static readonly string CustomerIdFieldName = "CustomerId";
        public static readonly string GroupDocByHeadCustomerIdSQL = "SELECT TOP 1000 * FROM c WHERE c.DocType = @DocType and c.HeadOfGroupCustomerId = @HeadOfGroupCustomerId Order By c._ts DESC";
        public static readonly string GroupDocFromHistoryByCustomerIdSQL = "SELECT distinct TOP 1000 a.SourceIdentifier, a.Brand, a.SchemaVersion, a.DocType, a.id, a.GroupId, a.CreatedOn, a.CreatedBy, a.LastUpdatedOn, a.LastUpdatedBy, a.GroupName, a.GroupTypeCode, a.Status, a.HeadOfGroupCustomerId, a.SourceSystem, a.GroupHistory, a._ts FROM a JOIN m IN a.GroupHistory WHERE a.DocType = @DocType AND m.CustomerId = @CustomerId ORDER BY a._ts DESC";
        public const string ConcurrentUpdateExceptionMessage1 = "one of the specified precondition is not met";
        public const string ConcurrentUpdateExceptionMessage2 = "One of the specified pre-condition is not met";
        public const int MaxNumberOfRetries = 1;
        public static readonly string SearchDbQuery = "SELECT * FROM c WHERE c.PartitionKey = @partitionKey and c.DocType = @docType";
        public static readonly string FlownMiles = "VSFLOWNMILES";
        public static readonly string StatusCode = "Processed";
        public static readonly int RedUpgradeTierPoints = 400;
        public static readonly int SilverUpgradeTierPoints = 1000;
        public static readonly int SilverRetainTierPoints = 400;
        public static readonly int GoldRetainTierPoints = 1000;
        public static readonly string VSTierPoints = "VSTierPoints";
        public static readonly string TierPoints = "TierPoints";
        public static readonly string MemberShipChangeDbQuery = "SELECT c.EndDate FROM c WHERE c.PartitionKey = @partitionKey and c.DocType = @docType order by c.LastUpdatedOn DESC";

        //CustomerProfile
        public static readonly string CustomerProfileSchemaVersion = "0.1";
        public static readonly string LoyaltyAuditPreferenceEventType = "VAA.Loyalty.Audit.PreferenceEvent";
        public static readonly string LoyaltyAuditPreferenceSubject = "VAA.Loyalty.Audit.PreferenceEvent";
        public static readonly string loyaltyAuditPreferenceSource = "auditeventmessages";
        public static readonly string Channel = "WEB-MYA";
        public static readonly string ContenTypeapplicationjson = "application/json";
        public static readonly string DoNotCall = "DoNotCall";
        public static readonly string DoNotSMS = "DoNotSMS";
        public static readonly string DoNotEmail = "DoNotEmail";
        public static readonly string VAAMarketingPermissions = "VAAMarketingPermissions";
        public const string preferenceEnabled = "Y";
        public const string preferenceDisabled = "N";
        public static readonly string UpdateAddressEmailEventType = "UpdateAddressEmailEventType";
        public static readonly string ManagaeAccountUpdateTopicName = "manageaccount";
        public static readonly string UpdateCustomerEventSubject = "MyAccountEmailEvent";
        public static readonly string UpdateMarketingPreferenceTemplate = "vaaLoyaltyFCGEN14i";

        // Partner Membership
        public static readonly string PartnerMembershipBrand = "VAA";
        public static readonly string PartnerMembershipDocSchemaVersion = "0.1";
        public static readonly string CenturianFlag = "Y";
        public const string PartnerMembershipEventTopicName = "customertransactioneventmessages";
        public const string CreatePartnerMembershipEventSubscriptionName = "CreatePartnerMembershipSubscriber";
        public const string UpdatePartnerMembershipEventSubscriptionName = "UpdatePartnerMembershipSubscriber";
        public static readonly string DisableDowngradeProhibitFlag = "N";
        public static readonly string DisableCenturionFlag = "N";

        //CustomerSearch
        public static readonly string CustomerSearchSchemaVersion = "0.1";
        public static readonly string CustomerSearchTopicName = "customereventmessages";

        //Cancel Transaction
        public static readonly string[] AccrualCancellationReasons = { "Other", "Canx Wrong Account Credited", "Canx Agent Error", "Canx Wrong Class Credit", "Gift Voucher Cancel", "Within 24h of travel", "Agent Error (Supervisor only)", "Outside 24h of travel" };
        public static readonly string[] RedemptionCancellationReasons = { "Class Change", "Within 24h of travel", "Agent Error (Supervisor only)", "Points + Money to full revenue", "Outside 24h of travel", "Canx due to health reasons", "Route Change", "Gift Voucher Cancel", "Suspected Fraud", "PPM - No refund of points", "Other" };
        public const string AccrualCancellation = "Accrual Cancellation";
        public const string RedemptionCancellation = "Redemption Cancellation";
        public const string CancelTransactionStatus = "Cancelled";
        public static readonly string RedemptionTransactionType = "Redemption";
        public const string FlightProductName = "Flight";
        public const string VoucherProductName = "Voucher Gifting";
        public const string VoucherGiftProductName = "Voucher Gifted";
        public const string VSFlightPartnerCode = "VS";
        public const string VSFlightPartnerName = "Virgin Atlantic Airways";
        public const string VoucherPointsProductName = "Voucher Points";

        public static readonly string CreateBannerResponseType = "CreateBannerResponse";
        public static readonly string BannerEventSubject = "BannerEventSubject";
        public static readonly string BannerChannelType = "WEB";
        public const string CreateBannerEventSubscriptionName = "CreateBannerEventSubscriber";
        public const string UpdateBannerEventSubscriptionName = "UpdateBannerEventSubscriber";
        public static readonly string DateMonthYearFormat = "dd-MMM-yyyy";

        public const string CreateMemberCustomAttributesEventSubscriptionName = "CreateMemberCustomAttributesEventSubscriber";
        public const string UpdateMemberCustomAttributesEventSubscriptionName = "UpdateMemberCustomAttributesEventSubscriber";
        public const int MemberActivityYearCount = 5;

        // Reference Data
        public const string ReferenceDataTopicName = "customertransactioneventmessages";
        public const string ProductDataSubscriptionName = "ProductDataMessageSubscriber";
        public const string ProductPartitionKey = "Product";
        public static readonly string ReferenceTableName = "ReferenceDataTable";
        public const string PartnerDataSubscriptionName = "PartnerDataMessageSubscriber";
        public const string PartnerPartitionKey = "Partner";

        //Vouchers
        public static readonly string ISODateFormat = "yyyy-MM-dd";
        public const int RedBaseValue = 75000;
        public const int SilverBaseValue = 150000;
        public const int GoldBaseValue = 150000;
        public const int WingsAvailable = 200000;
        public static readonly string VirginMoney = "Virgin Money";
        public const string ProductTypeElectronicVoucher = "Electronic Voucher";
        public const string WingsMemProductName = "Top Member";
        public const string Available = "Available";
        public const string PartiallyUsed = "Partially Used";
        public const string Expired = "Expired";
        public const string Used = "Used";

        public const string RedeemVoucherEventSubscriptionName = "RedeemVoucherEventSubscriber";
        public const string TxnSubType = "Product";
        public const string BaseMilesType = "BaseMiles";
        public const string MilesType = "Miles";
        public const string MilesQualifyingFlag = "N";
        public static readonly string[] MemberStatusesForAccrual = { "Active", "Frozen", "ActivePending", "Expired", "Inactive" };
        public const string CabinBonusType = "CabinBonus";
        public const string TierBonusType = "TierBonus";
        public const string PromotionBonusType = "PromotionBonus";
        public const string PointType = "Points";
        public const string CreateTransaction = "CreateTransaction";
        public const string CancelTransaction = "CancelTransaction";
        public const string RollbackVoucherRdmEventSubscriptionName = "RollbackVoucherRedemptionEventSubscriber";
        public static readonly string RollbackReasonCode = "Other";
        public static readonly string VoucherRedemptionCommandResponseStatusKey = "VoucherRedemption";
        public const string UpdateTransactionsPnr = "updateVoucherRelatedTxnsPnr";
        public const string UpdVchrRelatedTxnsPnrEventSubscriptionName = "UpdateVoucherRelatedTxnsPnrEventSubscriber";

        // Voucher Monitoring
        public const string MainRedemptionProduct = "Flight Redemption";
        public const string VoucherMonitoringSubscriptionName = "VoucherUsageProactiveMonitoringEventSubscriber";
    }
}
