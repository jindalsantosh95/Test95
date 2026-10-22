using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Audit.Validators;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Command.CardLinkingAudit;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Entities;
namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CardLinkingAudit
{
    public class CardLinkingAuditCreateEventHandler : AuditBaseEventHandler, IRequestHandler<CardLinkingAuditCreateEvent, Result<Unit>>
    {
        public CardLinkingAuditCreateEventHandler(IMediator mediator) : base(mediator)
        { }

        public Task<Result<Unit>> Handle(CardLinkingAuditCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(AuditBaseEvent auditEvent, CancellationToken cancellationToken)
        {
            CardLinkingAuditCommandBase createEvent = Json.DeserializeObject<CardLinkingAuditCommandBase>((JToken)auditEvent.Data);

            var validationResult = new CardLinkingAuditValidator().Validate(createEvent.CardLinkingAudit);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Card Linking create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            CardLinkingAuditDTO cardLinkingAuditDto = (CardLinkingAuditDTO)createEvent.CardLinkingAudit;

            cardLinkingAuditDto.Id = auditEvent.Id;

            return Task.FromResult(new CosmosDocumentEvent(cardLinkingAuditDto, auditEvent.EventType));
        }
    }
}
