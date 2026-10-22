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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.ContactNotes
{
    public class ContactNoteDeleteEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public ContactNoteDeleteEvent()
        {
            EventType = CosmosModificationType.Delete;
        }
    }
    public class ContactNoteDeleteEventHandler : TransactionBaseEventHandler, IRequestHandler<ContactNoteDeleteEvent, Result<Unit>>
    {
        public ContactNoteDeleteEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(ContactNoteDeleteEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            ContactNoteCommandBase deleteEvent = Json.DeserializeObject<ContactNoteCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new ContactNoteDeleteValidator().Validate(deleteEvent.ContactNote);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            // Check the note exists
            var deleteNote = (await ContactNoteCommandBase.GetListOfContactNotes(_mediator, deleteEvent.CustomerId, cancellationToken))
                                                        .FirstOrDefault(n => n.Id == deleteEvent.ContactNote.Id);
            if (deleteNote is null)
                throw new RequestValidationException(ErrorCodes.ContactNoteIdInvalid);

            ContactNoteDTO dto = new(deleteNote);

            // Complete the DTO
            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
