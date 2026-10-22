using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.ContactNotes
{
    public class ContactNotesQueryHandler : TransactionBaseQueryHandler<ContactNotesResponse>, IRequestHandler<ContactNotesQuery, Result<Response<ContactNotesResponse>>>
    {
        public ContactNotesQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<ContactNotesResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<ContactNotesResponse>>> Handle(ContactNotesQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
