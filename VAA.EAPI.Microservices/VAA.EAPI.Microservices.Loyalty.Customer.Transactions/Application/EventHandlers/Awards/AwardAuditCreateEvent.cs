using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Awards
{
    public class AwardAuditEvent
    {
        public AwardAuditDTO awardAudit { get; set; }
    }
}
