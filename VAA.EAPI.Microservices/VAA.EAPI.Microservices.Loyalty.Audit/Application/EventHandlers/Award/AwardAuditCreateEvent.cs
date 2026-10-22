using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.Award
{
    public class AwardAuditCreateEvent : AuditBaseEvent, IRequest<Result<Unit>>
    {
        public AwardAuditCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
}
