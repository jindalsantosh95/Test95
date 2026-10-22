using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.PreferenceAudit
{
    public class PreferenceAuditCommandBase : AuditBaseCommand
    {
        public Domain.Entities.PreferenceAudit PreferenceAuditDocument { get; set; }

        public PreferenceAuditCommandBase() : base()
        {
            Subject = ConfigReader.Get(Environment.PreferenceAuditDocType);
            DocType = ConfigReader.Get(Environment.PreferenceAuditDocType);
        }
    }
}
