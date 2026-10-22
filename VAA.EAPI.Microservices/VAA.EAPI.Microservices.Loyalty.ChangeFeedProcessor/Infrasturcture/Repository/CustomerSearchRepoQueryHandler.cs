using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository
{
    public class CustomerSearchRepoQueryHandler : IRequestHandler<CustomerSearchRepoQuery, IEnumerable<object>>
    {
        protected readonly IMediator _mediator;

        public CustomerSearchRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<object>> Handle(CustomerSearchRepoQuery request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(ConfigureCosmosQuery(request), cancellationToken);
        }

        private CosmosReaderRepoQuery ConfigureCosmosQuery(CustomerSearchRepoQuery request)
        {
            string sqlQuery = Constants.Application.CustomerSearchSqlQuery;

            return new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustSearchDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustSearchDBName),
                CollectionName = ConfigReader.Get(Constants.Environment.SCVCustSearchDBCollection),
                QueryParams = ConfigureQueryParams(request),
                SqlQuery = sqlQuery
            };
        }

        private List<KeyValuePair<string, object>> ConfigureQueryParams(CustomerSearchRepoQuery request)
        {
            return [
                    new KeyValuePair<string, object>("PartitionKey", request.DocType),
                    new KeyValuePair<string, object>("LoyaltyMemberNumber", request.MemberNumber)
                    ];
        }
    }
}
