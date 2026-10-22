using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.PartnerMembership
{
    public class PartnerMembershipAuditEvent
    {
        public CardLinkingAuditDTO CardLinkingAudit { get; set; }
    }
}
