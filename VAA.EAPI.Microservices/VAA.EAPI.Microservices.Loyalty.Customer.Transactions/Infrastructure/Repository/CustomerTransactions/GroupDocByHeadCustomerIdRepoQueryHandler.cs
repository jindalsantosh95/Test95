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
    public class GroupDocByHeadCustomerIdRepoQueryHandler : TransactionRepoQueryHandlerBase, IRequestHandler<GroupDocByHeadCustomerIdRepoQuery, IEnumerable<object>>
    {
        public GroupDocByHeadCustomerIdRepoQueryHandler(IMediator mediator, ILogger<TransactionRepoQueryHandlerBase> logger) : base(mediator, logger) { }


        public async Task<IEnumerable<object>> Handle(GroupDocByHeadCustomerIdRepoQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override CosmosReaderRepoQuery ConfigureCosmosQuery(CustomerTransactionRepoQuery request)
        {
            List<KeyValuePair<string, object>> queryParams = new()
                {
                    new KeyValuePair<string, object>(Constants.Application.HeadOfGroupCustomerIdFieldName, ((GroupDocByHeadCustomerIdRepoQuery)request).HeadCustomerId),
                    new KeyValuePair<string, object>(Constants.Application.DocTypeFieldName, request.DocType)
                };

            CosmosReaderRepoQuery cosmosReaderRepoQuery = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDB),
                CollectionName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBCollection),
                QueryParams = new List<KeyValuePair<string, object>>(queryParams),
                SqlQuery = Constants.Application.GroupDocByHeadCustomerIdSQL
            };

            return cosmosReaderRepoQuery;

        }
    }
}
