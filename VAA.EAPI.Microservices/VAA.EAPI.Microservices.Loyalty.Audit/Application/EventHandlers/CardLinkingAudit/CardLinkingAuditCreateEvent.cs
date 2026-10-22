using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CardLinkingAudit
{
    public class CardLinkingAuditCreateEvent : AuditBaseEvent, IRequest<Result<Unit>>
    {
        public CardLinkingAuditCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
}
