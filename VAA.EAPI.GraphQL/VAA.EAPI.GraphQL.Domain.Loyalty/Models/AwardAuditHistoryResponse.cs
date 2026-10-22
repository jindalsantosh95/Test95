using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class AwardAuditHistoryResponse
    {
        public IEnumerable<AwardAudit> AwardAuditHistory { get; set; }
    }
}