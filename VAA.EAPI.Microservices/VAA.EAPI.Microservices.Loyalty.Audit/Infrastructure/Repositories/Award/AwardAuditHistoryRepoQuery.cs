using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Award
{
    public class AwardAuditHistoryRepoQuery : IRequest<IEnumerable<AwardAudit>>
    {
        public string CustomerId { get; set; }

        public AwardAuditHistoryRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}