using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions.InviteHouseHold
{
    public class InviterHouseHoldRepoQueryHandler : TransactionRepoQueryHandlerBase, IRequestHandler<InviterHouseHoldRepoQuery, IEnumerable<object>>
    {
        public InviterHouseHoldRepoQueryHandler(IMediator mediator, ILogger<TransactionRepoQueryHandlerBase> logger) : base(mediator, logger) { }

        public async Task<IEnumerable<object>> Handle(InviterHouseHoldRepoQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CosmosReaderRepoQuery ConfigureCosmosQuery(CustomerTransactionRepoQuery request)
        {
            List<KeyValuePair<string, object>> queryParams = new()
                {
                    new KeyValuePair<string, object>(((InviterHouseHoldRepoQuery)request).InviteParameter, ((InviterHouseHoldRepoQuery)request).InviterMemberNumber),
                    new KeyValuePair<string, object>(Constants.Application.PartitionKeyFieldName, Constants.Application.InviteHouseHoldDocType)
                };

            return new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(EnvironmentSettings.TransactionDBEndPoint),
                DatabaseName = ConfigReader.Get(EnvironmentSettings.TransactionDB),
                CollectionName = ConfigReader.Get(EnvironmentSettings.TransactionDBCollection),
                QueryParams = new List<KeyValuePair<string, object>>(queryParams),
                SqlQuery = ((InviterHouseHoldRepoQuery)request).SQLQuery
            };
        }
    }
}
