using VAA.EAPI.Microservices.Loyalty.Audit.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.CustomerProfileAudit
{
    public class CustomerProfileAuditCommandBase : AuditBaseCommand
    {
        public Domain.Entities.CustomerProfileAudit CustomerProfileAuditDocument { get; set; }

        public CustomerProfileAuditCommandBase() : base()
        {
            Subject = DocumentType.CustomerProfileAudit.ToString();
            DocType = DocumentType.CustomerProfileAudit.ToString();
        }
    }
}