using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction
{
    public class RelatedTransactionQuery : TransactionBase, IRequest<Result<Response<TransactionResponse>>>
    {
        public string TransactionId { get; set; }
    }
}
