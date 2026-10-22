using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class CustomerProfileAuditHistoryQuery : IRequest<IEnumerable<CustomerProfileAudit>>
    {
        public string CustomerId { get; set; }

        public CustomerProfileAuditHistoryQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}