namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Audit
{
    public class SupplementarySilverAuditEvent
    {
        public LoyaltyGenericAudit LoyaltyGenericAuditDocument { get; set; }

        public static explicit operator SupplementarySilverAuditEvent(LoyaltyGenericAudit genericAudit)
        {
            return new SupplementarySilverAuditEvent()
            {
                LoyaltyGenericAuditDocument = genericAudit
            };
        }
    }
}