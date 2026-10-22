using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ContactNotes
{
    public class ContactNotePatchCommand : ContactNoteCommandBase, IRequest<Result<Response<ContactNoteCommandResponse>>>
    {
        public ContactNotePatchCommand()
        {
            EventType = TransactionCommandType.UpdateContactNote;

        }
    }
    public class ContactNotePatchCommandHandler : TransactionBaseCommandHandler<ContactNoteCommandResponse>, IRequestHandler<ContactNotePatchCommand, Result<Response<ContactNoteCommandResponse>>>
    {
        public ContactNotePatchCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<ContactNoteCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<ContactNoteCommandResponse>>> Handle(ContactNotePatchCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        //protected override async Task<TransactionBaseCommand> ValidateRequest(TransactionBaseCommand command, CancellationToken cancellationToken)
        //{
        //    var validatedCommand = (ContactNoteCommandBase)command;

        //    // Validate
        //    if (!new ContactNotePatchValidator().Validate(validatedCommand).IsValid)
        //        throw new RequestValidationException("Contact Note update request contains invalid data or missing mandatory fields.");

        //    // Get list of notes
        //    var noteList = await ContactNoteCommandBase.GetListOfContactNotes(_mediator, command.CustomerId, cancellationToken);

        //    // Find the one to patch 
        //    var noteToPatch = noteList.FirstOrDefault(note => note.Id == ((ContactNotePatchCommand)command).ContactNote.Id);
        //    if (noteToPatch is null)
        //        throw new RequestValidationException("Unable to find contact note record for Update Request.");

        //    // Copy over the requested updates
        //    noteToPatch.LastUpdatedOn = DateTime.Now;
        //    noteToPatch.LastUpdatedBy = validatedCommand.Originator;
        //    noteToPatch.Text = validatedCommand.ContactNote.Text;
        //    noteToPatch.Group = validatedCommand.ContactNote.Group;
        //    noteToPatch.Type = validatedCommand.ContactNote.Type;

        //    validatedCommand.ContactNote = noteToPatch;
        //    return validatedCommand;
        //}
    }

}