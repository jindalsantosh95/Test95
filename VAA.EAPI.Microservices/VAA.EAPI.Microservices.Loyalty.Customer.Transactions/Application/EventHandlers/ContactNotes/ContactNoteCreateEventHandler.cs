using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.ContactNotes
{
    public class ContactNoteCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public ContactNoteCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
    public class ContactNoteCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<ContactNoteCreateEvent, Result<Unit>>
    {
        public ContactNoteCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(ContactNoteCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            ContactNoteCommandBase createEvent = Json.DeserializeObject<ContactNoteCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new ContactNoteCreateValidator().Validate(createEvent.ContactNote);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            ContactNoteDTO dto = new(createEvent.ContactNote)
            {
                // Fill in the blanks (for a new record)
                CustomerId = createEvent.CustomerId,
                PartitionKey = createEvent.CustomerId,
                Id = transactionEvent.Id,
                NoteId = transactionEvent.Id,
                CreatedBy = !string.IsNullOrEmpty(transactionEvent.Originator) ? transactionEvent.Originator : createEvent.ContactNote.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = !string.IsNullOrEmpty(transactionEvent.Originator) ? transactionEvent.Originator : createEvent.ContactNote.CreatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),

            };

            return Task.FromResult(new CosmosDocumentEvent(dto, transactionEvent.EventType));
        }
    }


}
