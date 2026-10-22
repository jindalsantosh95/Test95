using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class CustomerTransactionRepoQueryHandler : TransactionRepoQueryHandlerBase, IRequestHandler<CustomerTransactionRepoQuery, IEnumerable<object>>
    {
        public CustomerTransactionRepoQueryHandler(IMediator mediator, ILogger<TransactionRepoQueryHandlerBase> logger) : base(mediator, logger) { }
    }
}
