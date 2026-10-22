using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class AwardAuditHistoryQuery : IRequest<IEnumerable<AwardAudit>>
    {
        public string CustomerId { get; set; }

        public AwardAuditHistoryQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}