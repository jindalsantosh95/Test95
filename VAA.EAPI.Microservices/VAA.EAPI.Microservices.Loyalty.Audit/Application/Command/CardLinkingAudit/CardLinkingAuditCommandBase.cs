using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.CardLinkingAudit
{
    public class CardLinkingAuditCommandBase : AuditBaseCommand
    {
        public Domain.Entities.CardLinkingAudit CardLinkingAudit { get; set; }

        public CardLinkingAuditCommandBase() : base()
        {
            Subject = ConfigReader.Get(Environment.CardLinkingAuditDocType);
            DocType = ConfigReader.Get(Environment.CardLinkingAuditDocType);
        }

    }
}
