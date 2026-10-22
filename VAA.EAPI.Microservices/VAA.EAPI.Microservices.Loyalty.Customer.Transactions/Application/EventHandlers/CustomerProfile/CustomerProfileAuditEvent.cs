using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.CustomerProfile
{
    public class CustomerProfileAuditEvent
    {
        public CustomerProfileAuditDTO CustomerProfileAuditDocument { get; set; }
    }
}
