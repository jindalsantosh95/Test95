using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Preference
{
    public class PreferenceAuditHistoryRepoQuery : IRequest<IEnumerable<PreferenceAudit>>
    {
        public string CustomerId { get; set; }

        public PreferenceAuditHistoryRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}