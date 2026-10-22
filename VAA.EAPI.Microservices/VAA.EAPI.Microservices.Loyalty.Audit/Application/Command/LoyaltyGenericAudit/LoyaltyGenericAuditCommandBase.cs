using VAA.EAPI.Microservices.Loyalty.Audit.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.LoyaltyGenericAudit
{
    public class LoyaltyGenericAuditCommandBase : AuditBaseCommand
    {
        public Domain.Entities.LoyaltyGenericAudit LoyaltyGenericAuditDocument { get; set; }

        public LoyaltyGenericAuditCommandBase() : base()
        {
            Subject = DocumentType.OtherAudit.ToString();
            DocType = DocumentType.OtherAudit.ToString();
        }
    }
}