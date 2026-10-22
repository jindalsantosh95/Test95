using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.LoyaltyGenericAudit
{
    public class LoyaltyGenericAuditCreateEvent : AuditBaseEvent, IRequest<Result<Unit>>
    {
        public LoyaltyGenericAuditCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
}