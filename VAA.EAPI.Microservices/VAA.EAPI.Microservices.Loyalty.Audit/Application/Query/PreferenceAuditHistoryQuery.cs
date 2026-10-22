using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Query
{
    public class PreferenceAuditHistoryQuery : IRequest<IEnumerable<PreferenceAudit>>
    {
        public string CustomerId { get; set; }

        public PreferenceAuditHistoryQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}