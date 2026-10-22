using MediatR;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Audit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class SSAAuditCreationEventCommand : IRequest<Unit>
    {
        public LoyaltyGenericAudit LoyaltyGenericAudit { get; set; }

        public static explicit operator SSAAuditCreationEventCommand(LoyaltyGenericAudit genericAudit)
        {
            return new SSAAuditCreationEventCommand()
            {
                LoyaltyGenericAudit = genericAudit
            };
        }
    }
}