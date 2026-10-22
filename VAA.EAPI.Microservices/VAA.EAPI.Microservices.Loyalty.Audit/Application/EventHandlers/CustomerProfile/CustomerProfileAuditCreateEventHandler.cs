using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Audit.Validators;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.CustomerProfileAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CustomerProfile
{
    public class CustomerProfileAuditCreateEventHandler : AuditBaseEventHandler, IRequestHandler<CustomerProfileAuditCreateEvent, Result<Unit>>
    {
        public CustomerProfileAuditCreateEventHandler(IMediator mediator) : base(mediator)
        { }

        public Task<Result<Unit>> Handle(CustomerProfileAuditCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(AuditBaseEvent auditEvent, CancellationToken cancellationToken)
        {
            CustomerProfileAuditCommandBase createEvent = Json.DeserializeObject<CustomerProfileAuditCommandBase>((JToken)auditEvent.Data);

            var validationResult = new CustomerProfileAuditCreateValidator().Validate(createEvent.CustomerProfileAuditDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Customer Profile Audit create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            CustomerProfileAuditDTO dto = new(createEvent.CustomerProfileAuditDocument)
            {
                Id = auditEvent.Id,
            };

            return Task.FromResult(new CosmosDocumentEvent(dto, auditEvent.EventType));
        }
    }
}