namespace VAA.EAPI.Microservices.Loyalty.Audit.Constants
{
    public class Application
    {
        public static readonly string SearchDbQuery = "SELECT TOP 100 * FROM c WHERE c.PartitionKey = @partitionKey and c.DocType = @docType order by c.CreatedOn desc";
        public static readonly string LoyaltyGenericAuditSearchDbQuery = "SELECT TOP 100 * FROM c WHERE c.PartitionKey = @partitionKey and c.DocType = @docType and c.AuditCategory = @auditCategory order by c.CreatedOn desc";
        public static readonly string SupplementarySilverAuditCategory = "Supplementary Silver";

        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static readonly string CommandResponseStatusKey = "LoyaltyAudit";

        public const string AuditEventTopicName = "auditeventmessages";
        public const string AwardAuditEventSubscriptionName = "CreateAwardAuditEventSubscriber";
        public const string CreatePreferenceAuditEventSubscriptionName = "CreatePreferenceAuditEventSubscriber";
        public const string CreateCustomerProfileAuditEventSubscriptionName = "CreateCustomerProfileAuditEventSubscriber";
        public const string CreateLoyaltyGenericAuditEventSubscriptionName = "CreateOtherAuditEventSubscriber";
        public const string CardLinkingAuditSubscriberName = "CardLinkingAuditEventSubscriber";
    }
}