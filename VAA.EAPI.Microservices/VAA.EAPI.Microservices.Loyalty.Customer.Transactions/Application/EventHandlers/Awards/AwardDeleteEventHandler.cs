using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Awards
{
    public class AwardDeleteEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public AwardDeleteEvent()
        {
            EventType = CosmosModificationType.Delete;
        }
    }
    public class AwardDeleteEventHandler : TransactionBaseEventHandler, IRequestHandler<AwardDeleteEvent, Result<Unit>>
    {
        public AwardDeleteEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(AwardDeleteEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            AwardCommandBase deleteEvent = Json.DeserializeObject<AwardCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new AwardDeleteValidator().Validate(deleteEvent.Award);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            AwardDTO dto = new(deleteEvent.Award);


            // Complete the DTO
            return Task.FromResult(new CosmosDocumentEvent(dto, transactionEvent.EventType));
        }
    }
}
