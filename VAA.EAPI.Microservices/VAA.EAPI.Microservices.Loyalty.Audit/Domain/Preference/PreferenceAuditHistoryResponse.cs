using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.Preference
{
    public class PreferenceAuditHistoryResponse
    {
        public IEnumerable<PreferenceAudit> PreferenceHistoryAudit { get; set; }

        public PreferenceAuditHistoryResponse(IEnumerable<PreferenceAudit> preferenceHistoryDocuments)
        {
            PreferenceHistoryAudit = preferenceHistoryDocuments;
        }
    }
}
