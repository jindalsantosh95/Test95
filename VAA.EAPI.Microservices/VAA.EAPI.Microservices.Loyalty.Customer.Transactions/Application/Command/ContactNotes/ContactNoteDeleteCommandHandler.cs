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
    public class ContactNoteDeleteCommand : ContactNoteCommandBase, IRequest<Result<Response<ContactNoteCommandResponse>>>
    {
        public ContactNoteDeleteCommand()
        {
            EventType = TransactionCommandType.DeleteContactNote;

        }
    }
    public class ContactNoteDeleteCommandHandler : TransactionBaseCommandHandler<ContactNoteCommandResponse>, IRequestHandler<ContactNoteDeleteCommand, Result<Response<ContactNoteCommandResponse>>>
    {
        public ContactNoteDeleteCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<ContactNoteCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<ContactNoteCommandResponse>>> Handle(ContactNoteDeleteCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        //protected override async Task<TransactionBaseCommand> ValidateRequest(TransactionBaseCommand command, CancellationToken cancellationToken)
        //{
        //    var validatedCommand = (ContactNoteCommandBase)command;

        //    // Validate
        //    if (!new ContactNoteDeleteValidator().Validate(validatedCommand).IsValid)
        //        throw new RequestValidationException("Contact note delete request contains invalid data or missing mandatory fields.");

        //    // Get list of notes
        //    var noteList = await ContactNoteCommandBase.GetListOfContactNotes(_mediator, command.CustomerId, cancellationToken);

        //    // Find the one to patch 
        //    if (!noteList.Any(note => note.Id == ((ContactNoteDeleteCommand)command).ContactNote.Id))
        //        throw new RequestValidationException("Unable to find contact note record for delete request.");

        //    return validatedCommand;
        //}
    }
}
