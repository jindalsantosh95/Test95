using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.AmexCenturion
{
    public class AmexCenturionAuditHistoryRepoQuery : IRequest<IEnumerable<CardLinkingAudit>>
    {
        public string CustomerId { get; set; }

        public AmexCenturionAuditHistoryRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}

