using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction
{
    public class TransactionCreateCommand : TransactionCommandBase, IRequest<Result<Response<TransactionCommandResponse>>>
    {
        public TransactionCreateCommand() : base()
        {
            EventType = TransactionCommandType.CreateTransaction;
        }
    }
    public class TransactionCreateCommandHandler : TransactionBaseCommandHandler<TransactionCommandResponse>, IRequestHandler<TransactionCreateCommand, Result<Response<TransactionCommandResponse>>>
    {
        public TransactionCreateCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<TransactionCommandResponse>> logger) :
            base(mediator, logger)
        {
        }

        public Task<Result<Response<TransactionCommandResponse>>> Handle(TransactionCreateCommand request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }
    }
}
