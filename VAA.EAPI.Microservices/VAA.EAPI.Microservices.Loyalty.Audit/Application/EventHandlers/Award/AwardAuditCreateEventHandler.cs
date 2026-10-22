using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Audit.Validators;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.AwardsAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.Award
{
    public class AwardAuditCreateEventHandler : AuditBaseEventHandler, IRequestHandler<AwardAuditCreateEvent, Result<Unit>>
    {
        public AwardAuditCreateEventHandler(IMediator mediator) : base(mediator)
        { }

        public Task<Result<Unit>> Handle(AwardAuditCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(AuditBaseEvent auditEvent, CancellationToken cancellationToken)
        {
            AwardAuditCommandBase createEvent = Json.DeserializeObject<AwardAuditCommandBase>((JToken)auditEvent.Data);

            var validationResult = new AwardAuditValidator().Validate(createEvent.AwardAudit);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Award audit create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            AwardAuditDTO dto = new(createEvent.AwardAudit)
            {
                Id = auditEvent.Id
            };

            return Task.FromResult(new CosmosDocumentEvent(dto, auditEvent.EventType));
        }
    }
}
