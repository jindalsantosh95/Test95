using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class InviteHouseHoldRepoQueryHandler : TransactionRepoQueryHandlerBase, IRequestHandler<InviteHouseHoldRepoQuery, IEnumerable<object>>
    {
        public InviteHouseHoldRepoQueryHandler(IMediator mediator, ILogger<TransactionRepoQueryHandlerBase> logger) : base(mediator, logger) { }


        public async Task<IEnumerable<object>> Handle(InviteHouseHoldRepoQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CosmosReaderRepoQuery ConfigureCosmosQuery(CustomerTransactionRepoQuery request)
        {
            List<KeyValuePair<string, object>> queryParams = new()
            {
                new KeyValuePair<string, object>(Constants.Application.InviteeMemberNumber, ((InviteHouseHoldRepoQuery)request).InviteeMemberNumber),
                new KeyValuePair<string, object>(Constants.Application.PartitionKeyFieldName, Constants.Application.InviteHouseHoldDocType)
            };

            CosmosReaderRepoQuery cosmosReaderRepoQuery = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDB),
                CollectionName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBCollection),
                QueryParams = new List<KeyValuePair<string, object>>(queryParams),
                SqlQuery = Constants.Application.InviteHouseHoldDbQueryforInvitee
            };

            return cosmosReaderRepoQuery;

        }
    }
}
