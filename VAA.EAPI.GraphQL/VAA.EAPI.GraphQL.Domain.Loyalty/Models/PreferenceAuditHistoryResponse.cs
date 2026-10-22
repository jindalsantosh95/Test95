using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class PreferenceAuditHistoryResponse
    {
        public IEnumerable<PreferenceAudit> PreferenceHistoryAudit { get; set; }
    }
}