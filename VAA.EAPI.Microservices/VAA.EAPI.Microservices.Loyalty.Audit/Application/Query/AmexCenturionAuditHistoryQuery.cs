using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class AmexCenturionAuditHistoryQuery : IRequest<IEnumerable<CardLinkingAudit>>
    {
        public string CustomerId { get; set; }

        public AmexCenturionAuditHistoryQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
