using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction
{
    public class TransactionQuery : TransactionBase, IRequest<Result<Response<TransactionResponse>>>
    { }
    public class TransactionDocumentQueryHandler : TransactionBaseQueryHandler<TransactionResponse>, IRequestHandler<TransactionQuery, Result<Response<TransactionResponse>>>
    {
        public TransactionDocumentQueryHandler(IMediator mediator, ILogger<TransactionBaseQueryHandler<TransactionResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<TransactionResponse>>> Handle(TransactionQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }
    }
}
