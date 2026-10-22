using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.CustomerProfile
{
    public class CustomerProfileAuditHistoryRepoQuery : IRequest<IEnumerable<CustomerProfileAudit>>
    {
        public string CustomerId { get; set; }

        public CustomerProfileAuditHistoryRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}