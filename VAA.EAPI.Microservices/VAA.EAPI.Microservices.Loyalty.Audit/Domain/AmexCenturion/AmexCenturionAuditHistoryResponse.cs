using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.AmexCenturion
{
    public class AmexCenturionAuditHistoryResponse
    {
        public IEnumerable<CardLinkingAudit> AmexCenturionAuditHistory { get; set; }

        public AmexCenturionAuditHistoryResponse(IEnumerable<CardLinkingAudit> amexCenturionAuditDocuments)
        {
            AmexCenturionAuditHistory = amexCenturionAuditDocuments;
        }
    }
}
