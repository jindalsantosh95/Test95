using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class AmexCenturionAuditHistoryResponse
    {
        public IEnumerable<CardLinkingAudit> AmexCenturionAuditHistory { get; set; }
    }
}
