using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class SupplementarySilverAuditHistoryResponse
    {
        public IEnumerable<LoyaltyGenericAudit> SupplementarySilverAuditHistory { get; set; }
    }
}