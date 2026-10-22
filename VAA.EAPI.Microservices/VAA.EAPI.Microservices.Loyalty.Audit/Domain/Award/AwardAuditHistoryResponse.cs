using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.Award
{
    public class AwardAuditHistoryResponse
    {
        public IEnumerable<AwardAudit> AwardAuditHistory { get; set; }

        public AwardAuditHistoryResponse(IEnumerable<AwardAudit> awardAuditDocuments)
        {
            AwardAuditHistory = awardAuditDocuments;
        }
    }
}