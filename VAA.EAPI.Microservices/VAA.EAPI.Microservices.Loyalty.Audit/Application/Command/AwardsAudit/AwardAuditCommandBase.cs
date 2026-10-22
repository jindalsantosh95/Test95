using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Constants;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.AwardsAudit
{
    public class AwardAuditCommandBase : AuditBaseCommand
    {
        public AwardAudit AwardAudit { get; set; }

        public AwardAuditCommandBase() : base()
        {
            Subject = ConfigReader.Get(Environment.AwardAuditDocType);
            DocType = ConfigReader.Get(Environment.AwardAuditDocType);
        }
    }
}
