using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CustomerProfile
{
    public class CustomerProfileAuditCreateEvent : AuditBaseEvent, IRequest<Result<Unit>>
    {
        public CustomerProfileAuditCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
}