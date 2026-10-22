using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class GroupDocRepoQueryHandler : TransactionRepoQueryHandlerBase, IRequestHandler<GroupDocRepoQuery, IEnumerable<object>>
    {
        public GroupDocRepoQueryHandler(IMediator mediator, ILogger<TransactionRepoQueryHandlerBase> logger) : base(mediator, logger) { }


        public async Task<IEnumerable<object>> Handle(GroupDocRepoQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override List<KeyValuePair<string, object>> ConfigureQueryParams(CustomerTransactionRepoQuery request)
        {
            return new()
                {
                    new KeyValuePair<string, object>(Constants.Application.PartitionKeyFieldName, ((GroupDocRepoQuery)request).GroupId),
                    new KeyValuePair<string, object>(Constants.Application.DocTypeFieldName, request.DocType)
                };
        }
    }
}
