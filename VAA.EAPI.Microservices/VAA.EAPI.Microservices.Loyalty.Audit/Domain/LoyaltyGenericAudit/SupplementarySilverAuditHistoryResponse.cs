using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.LoyaltyGenericAudit
{
    public class SupplementarySilverAuditHistoryResponse
    {
        public IEnumerable<Entities.LoyaltyGenericAudit> SupplementarySilverAuditHistory { get; set; }

        public SupplementarySilverAuditHistoryResponse(IEnumerable<Entities.LoyaltyGenericAudit> loyaltyGenericAuditHistory)
        {
            SupplementarySilverAuditHistory = loyaltyGenericAuditHistory;
        }
    }
}