using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions.Banner
{
    public class BannerRepoQueryHandler : TransactionRepoQueryHandlerBase, IRequestHandler<BannerRepoQuery, IEnumerable<object>>
    {
        public BannerRepoQueryHandler(IMediator mediator, ILogger<TransactionRepoQueryHandlerBase> logger) : base(mediator, logger) { }

        public async Task<IEnumerable<object>> Handle(BannerRepoQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CosmosReaderRepoQuery ConfigureCosmosQuery(CustomerTransactionRepoQuery request)
        {
            List<KeyValuePair<string, object>> queryParams = new()
                {
                    new KeyValuePair<string, object>(Constants.Application.DocTypeFieldName, Constants.Application.BannerDocType ),
                    new KeyValuePair<string, object>(Constants.Application.PartitionKeyFieldName, ((BannerRepoQuery)request).CustomerId )
                };

            return new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(EnvironmentSettings.TransactionDBEndPoint),
                DatabaseName = ConfigReader.Get(EnvironmentSettings.TransactionDB),
                CollectionName = ConfigReader.Get(EnvironmentSettings.TransactionDBCollection),
                QueryParams = new List<KeyValuePair<string, object>>(queryParams),
                SqlQuery = ((BannerRepoQuery)request).SQLQuery
            };
        }
    }
}
