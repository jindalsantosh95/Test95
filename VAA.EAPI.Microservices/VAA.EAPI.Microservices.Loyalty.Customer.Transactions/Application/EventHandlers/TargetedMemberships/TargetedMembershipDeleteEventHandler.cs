using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.TargetedMemberships;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.TargetedMemberships
{
    public class TargetedMembershipDeleteEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public TargetedMembershipDeleteEvent()
        {
            EventType = CosmosModificationType.Delete;
        }
    }
    public class TargetedMembershipDeleteEventHandler : TransactionBaseEventHandler, IRequestHandler<TargetedMembershipDeleteEvent, Result<Unit>>
    {
        public TargetedMembershipDeleteEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(TargetedMembershipDeleteEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            TargetedMembershipCommandBase deleteEvent = Json.DeserializeObject<TargetedMembershipCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new TargetedMembershipDeleteValidator().Validate(deleteEvent.TargetedMembershipDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            // Check the note exists
            var deleteNote = (await TargetedMembershipCommandBase.GetListOfTargetedMemberships(_mediator, deleteEvent.CustomerId, cancellationToken))
                                                        .FirstOrDefault(n => n.Id == deleteEvent.TargetedMembershipDocument.Id);
            if (deleteNote is null)
                throw new RequestValidationException(Constants.ErrorCodes.TargetedMembershipIdInvalid);

            TargetedMembershipDTO dto = new(deleteNote);

            // Complete the DTO
            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
