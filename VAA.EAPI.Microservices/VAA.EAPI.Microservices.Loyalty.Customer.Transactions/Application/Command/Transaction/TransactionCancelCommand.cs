using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Transaction
{
    public class TransactionCancelCommand : TransactionCommandBase, IRequest<Result<Response<TransactionCommandResponse>>>
    {
        public TransactionCancelCommand() : base()
        {
            EventType = TransactionCommandType.CancelTransaction;
        }
    }
}
