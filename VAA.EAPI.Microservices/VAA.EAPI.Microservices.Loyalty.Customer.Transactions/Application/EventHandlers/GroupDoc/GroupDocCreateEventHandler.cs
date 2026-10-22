using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.GroupDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.GroupDoc
{
    public class GroupDocCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<GroupDocCreateEvent, Result<Unit>>
    {
        public GroupDocCreateEventHandler(IMediator mediator,
            ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(GroupDocCreateEvent request,
            CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            GroupDocCommandBase createCommand = Json.DeserializeObject<GroupDocCommandBase>((JToken)transactionEvent.Data);
            var validationResult = new GroupDocCreateCommandValidator().Validate(createCommand);
            if (!validationResult.IsValid)
            {
                _logger.LogError($"GroupDocument create request contains invalid data or missing mandatory fields. - {validationResult.Errors}");
                throw new RequestValidationException($"GroupDocument create request contains invalid data or missing mandatory fields. - {validationResult.Errors}");
            }

            return Task.FromResult(new CosmosDocumentEvent(new GroupDocumentDTO(createCommand.GroupDoc), transactionEvent.EventType));
        }
    }
}
