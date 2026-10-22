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
    public class ContactNoteCreateCommand : ContactNoteCommandBase, IRequest<Result<Response<ContactNoteCommandResponse>>>
    {
        public ContactNoteCreateCommand() : base()
        {
            EventType = TransactionCommandType.CreateContactNote;
        }
    }
    public class ContactNoteCreateCommandHandler : TransactionBaseCommandHandler<ContactNoteCommandResponse>, IRequestHandler<ContactNoteCreateCommand, Result<Response<ContactNoteCommandResponse>>>
    {
        public ContactNoteCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<ContactNoteCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<ContactNoteCommandResponse>>> Handle(ContactNoteCreateCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
