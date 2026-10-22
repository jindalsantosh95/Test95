using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction
{
    public class TransactionCancelCommandHandler : TransactionBaseCommandHandler<TransactionCommandResponse>, IRequestHandler<TransactionCancelCommand, Result<Response<TransactionCommandResponse>>>
    {
        public TransactionCancelCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<TransactionCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<TransactionCommandResponse>>> Handle(TransactionCancelCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
