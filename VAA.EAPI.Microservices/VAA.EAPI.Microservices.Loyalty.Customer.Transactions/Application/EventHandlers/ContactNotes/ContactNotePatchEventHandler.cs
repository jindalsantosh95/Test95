using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
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
    public class ContactNotePatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public ContactNotePatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }
    public class ContactNotePatchEventHandler : TransactionBaseEventHandler, IRequestHandler<ContactNotePatchEvent, Result<Unit>>
    {
        public ContactNotePatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(ContactNotePatchEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            ContactNoteCommandBase patchEvent = Json.DeserializeObject<ContactNoteCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new ContactNoteDeleteValidator().Validate(patchEvent.ContactNote);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note update request contains invalid data or missing mandatory fields.", validationResult.Errors);

            // Check the note exists
            var patchNote = (await ContactNoteCommandBase.GetListOfContactNotes(_mediator, patchEvent.CustomerId, cancellationToken))
                                                        .FirstOrDefault(n => n.Id == patchEvent.ContactNote.Id);
            if (patchNote is null)
                throw new RequestValidationException(ErrorCodes.ContactNoteIdInvalid);


            ContactNoteDTO dto = new(patchNote)
            {
                // Update the changed fields
                NoteTypeGroup = patchEvent.ContactNote.Group,
                NoteType = patchEvent.ContactNote.Type,
                Description = patchEvent.ContactNote.Text,
                LastUpdatedBy = !string.IsNullOrEmpty(transactionEvent.Originator) ? transactionEvent.Originator : patchEvent.ContactNote.LastUpdatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
            };

            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
