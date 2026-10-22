using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Audit.Validators;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.LoyaltyGenericAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.LoyaltyGenericAudit
{
    public class LoyaltyGenericAuditCreateEventHandler : AuditBaseEventHandler, IRequestHandler<LoyaltyGenericAuditCreateEvent, Result<Unit>>
    {
        public LoyaltyGenericAuditCreateEventHandler(IMediator mediator) : base(mediator)
        { }

        public Task<Result<Unit>> Handle(LoyaltyGenericAuditCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(AuditBaseEvent auditEvent, CancellationToken cancellationToken)
        {
            LoyaltyGenericAuditCommandBase createEvent = Json.DeserializeObject<LoyaltyGenericAuditCommandBase>((JToken)auditEvent.Data);

            var validationResult = new LoyaltyGenericAuditCreateEventValidator().Validate(createEvent.LoyaltyGenericAuditDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Loyalty Generic Audit create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            LoyaltyGenericAuditDTO loyaltyGenericAuditDto = (LoyaltyGenericAuditDTO)createEvent.LoyaltyGenericAuditDocument;
            loyaltyGenericAuditDto.Id = auditEvent.Id;

            return Task.FromResult(new CosmosDocumentEvent(loyaltyGenericAuditDto, auditEvent.EventType));
        }
    }
}