namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants
{
    public class EnvironmentSettings
    {
        // Enviromental settings
        public static readonly string CosmosSqlDBConnectionMode = "CosmosSqlDBConnectionMode";

        // Contact Notes
        public static readonly string ContactNoteDocType = "ContactNoteDocType";
        public static readonly string ContactNoteEventSubject = "ContactNoteEventSubject";

        // Targeted Membership
        public static readonly string TargetedMembershipDocType = "TargetedMembershipDocType";
        public static readonly string TargetedMembershipEventSubject = "TargetedMembershipEventSubject";

        // Transaction Document
        public static readonly string TransactionDocType = "TransactionDocType";
        public static readonly string TransactionEventSubject = "TransactionEventSubject";

        //Campaign Enrolment
        public static readonly string CampaignEnrolmentDocType = "CampaignEnrolmentDocType";
        public static readonly string CampaignEnrolmentEventSubject = "CampaignEnrolmentEventSubject";

        // Awards
        public static readonly string AwardDocType = "AwardDocType";
        public static readonly string TopMemberProductID = "TopMemberProductID";
        public static readonly string AwardEventSubject = "AwardEventSubject";

        // Flying Club Member
        public static readonly string FlyingClubMemberDocType = "FlyingClubMemberDocType";
        public static readonly string FlyingClubMemberEventSubject = "FlyingClubMemberEventSubject";

        // Transaction DB
        public static readonly string TransactionDBEndPoint = "TransactionDBEndPoint";
        public static readonly string TransactionDB = "TransactionDB";
        public static readonly string TransactionDBCollection = "TransactionCollection";

        // Service Bus
        public static readonly string ServiceBusConnectionString = "ServiceBusConnectionString";
        public static readonly string ServiceBusTopicName = "ServiceBusTopicName";

        // Command Status 
        public static readonly string CommandStatusLocation = "CommandStatusLocation";
        public static readonly string CommandStatusPollingDelayMS = "CommandStatusPollingDelayMS";
        public static readonly string CommandStatusPollingRetryCount = "CommandStatusPollingRetryCount";

        //Rule Engine
        public static readonly string RuleEngineTopicName = "RuleEngineTopicName";
        public static readonly string MembershipChangeDocType = "MembershipChangeDocType";

        //GroupDoc
        public static readonly string GroupDocType = "GroupDocType";
        public static readonly string GroupDocTypeEventSubject = "GroupDocTypeEventSubject";
        public static readonly string CustomerTransactionDB = "CustomerTransactionDB";
        public static readonly string CustomerTransactionCollection = "CustomerTransactionCollection";

        //Activity
        public static readonly string ActivityDocType = "ActivityDocType";

        // CustomerProfile
        public static readonly string CustomerProfileDocType = "CustomerProfileDocType";

        //PartnerMembership
        public static readonly string PartnerMembershipDocType = "PartnerMembershipDocType";
        public static readonly string PartnerMembershipEventSubject = "PartnerMembershipEventSubject";

        // Audit
        public static readonly string AuditServiceBusTopicName = "AuditServiceBusTopicName";

        //CustomerSearch
        public const string CustomerServiceBusTopicName = "CustomerSearchTopicName";

        //Banner
        public static readonly string BannerResponseURL = "BannerResponseURL";
        public const string ResponsePollingCount = "ResponsePollingCount";
        public const string ResponseDelayTimeInMS = "ResponseDelayTimeInMS";

        //MemberCustomAttributes
        public static readonly string MemberCustomAttributesDocType = "MemberCustomAttributesDocType";
        public static readonly string MemberCustomAttributesEventSubject = "MemberCustomAttributesEventSubject";

        // Reference Data
        public static readonly string MicroServiceDBKey = "MicroServiceDBKey";

        //Redeem Voucher
        public static readonly string CodeSharePartnerForStatementText = "CodeSharePartnerForStatementText";
        public static readonly string BlobConnStringForStatementTxt = "BlobConnStringForStatementTxt";
        public static readonly string ContainerNameForStatementTxt = "ContainerNameForStatementTxt";
        public static readonly string BlobNameForStatementTxt = "BlobNameForStatementTxt";
        public static readonly string LoyaltyMemberMgmtStorageSASKey = "LoyaltyMemberMgmtStorageSASKey";
        public static readonly string VredServiceBusTopicName = "VredServiceBusTopicName";
        public static readonly string VredCommandStatusLocation = "VredCommandStatusLocation";
        public static readonly string TransactionRecentWindowHours = "TransactionRecentWindowHours";
        public static readonly string CacheExpiryHours = "CacheExpiryHours";

        // Voucher Monitoring
        public static readonly string VoucherMonitoringDurationMinutes = "VoucherMonitoringDurationMinutes";
    }
}