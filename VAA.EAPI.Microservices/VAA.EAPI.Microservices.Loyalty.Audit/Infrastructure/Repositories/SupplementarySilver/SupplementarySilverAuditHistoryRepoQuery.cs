using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.SupplementarySilver
{
    public class SupplementarySilverAuditHistoryRepoQuery : IRequest<IEnumerable<LoyaltyGenericAudit>>
    {
        public string CustomerId { get; set; }

        public static explicit operator SupplementarySilverAuditHistoryRepoQuery(string customerId)
        {
            return new SupplementarySilverAuditHistoryRepoQuery()
            {
                CustomerId = customerId
            };
        }
    }
}