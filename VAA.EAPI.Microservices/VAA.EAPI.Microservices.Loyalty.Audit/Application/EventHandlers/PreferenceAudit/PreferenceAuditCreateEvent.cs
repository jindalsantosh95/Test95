using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.PreferenceAudit
{
    public class PreferenceAuditCreateEvent : AuditBaseEvent, IRequest<Result<Unit>>
    {
        public PreferenceAuditCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
}