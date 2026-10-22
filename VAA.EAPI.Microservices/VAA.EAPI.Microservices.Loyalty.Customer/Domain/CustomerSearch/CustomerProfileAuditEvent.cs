using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch
{
    public class CustomerProfileAuditEvent
    {
        public CustomerProfileAuditDTO CustomerProfileAuditDocument { get; set; }
    }
}
