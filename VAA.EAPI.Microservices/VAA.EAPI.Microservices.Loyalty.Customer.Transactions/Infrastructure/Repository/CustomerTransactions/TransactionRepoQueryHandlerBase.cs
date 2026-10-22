using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public abstract class TransactionRepoQueryHandlerBase
    {
        protected readonly IMediator _mediator;
        protected readonly ILogger<TransactionRepoQueryHandlerBase> _logger;

        public TransactionRepoQueryHandlerBase(IMediator mediator, ILogger<TransactionRepoQueryHandlerBase> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<object>> Handle(CustomerTransactionRepoQuery request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(ConfigureCosmosQuery(request), cancellationToken);
        }

        public virtual CosmosReaderRepoQuery ConfigureCosmosQuery(CustomerTransactionRepoQuery request)
        {

            return new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDB),
                CollectionName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBCollection),
                QueryParams = ConfigureQueryParams(request),
                SqlQuery = Constants.Application.CustomerTransactionsSQL.Replace("{fieldlist}", request.FieldList.Any() ? string.Join(",", request.FieldList) : "*")
            };
        }

        public virtual List<KeyValuePair<string, object>> ConfigureQueryParams(CustomerTransactionRepoQuery request)
        {
            return new()
            {
                new KeyValuePair<string, object>(Constants.Application.PartitionKeyFieldName, request.CustomerId),
                new KeyValuePair<string, object>(Constants.Application.DocTypeFieldName, request.DocType)
            };
        }
    }
}
