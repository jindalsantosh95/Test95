using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Audit.Validators;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.PreferenceAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.PreferenceAudit
{
    public class PreferenceAuditCreateEventHandler : AuditBaseEventHandler, IRequestHandler<PreferenceAuditCreateEvent, Result<Unit>>
    {
        public PreferenceAuditCreateEventHandler(IMediator mediator) : base(mediator)
        { }

        public Task<Result<Unit>> Handle(PreferenceAuditCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(AuditBaseEvent auditEvent, CancellationToken cancellationToken)
        {
            PreferenceAuditCommandBase createEvent = Json.DeserializeObject<PreferenceAuditCommandBase>((JToken)auditEvent.Data);

            var validationResult = new PreferenceAuditCreateValidator().Validate(createEvent.PreferenceAuditDocument);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Preference Audit create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            PreferenceAuditDTO dto = new(createEvent.PreferenceAuditDocument)
            {
                Id = auditEvent.Id,
            };

            return Task.FromResult(new CosmosDocumentEvent(dto, auditEvent.EventType));
        }
    }
}