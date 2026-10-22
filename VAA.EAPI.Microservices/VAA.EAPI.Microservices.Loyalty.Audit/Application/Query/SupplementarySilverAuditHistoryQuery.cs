using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class SupplementarySilverAuditHistoryQuery : IRequest<IEnumerable<LoyaltyGenericAudit>>
    {
        public string CustomerId { get; set; }

        public SupplementarySilverAuditHistoryQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}