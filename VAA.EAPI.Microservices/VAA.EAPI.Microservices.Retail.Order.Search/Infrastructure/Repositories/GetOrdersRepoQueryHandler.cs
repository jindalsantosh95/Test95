using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories
{
    public class GetOrdersRepoQueryHandler : IRequestHandler<OrdersRepoQuery, IEnumerable<OrderDoc>>
    {
        private readonly IMediator _mediator;

        public GetOrdersRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<OrderDoc>> Handle(OrdersRepoQuery request, CancellationToken cancellationToken)
        {

            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.OrdersCollection),
                SqlQuery = Constants.Application.Query,
                QueryParams = new()
                {
                    new KeyValuePair<string, object>("OrderID", request.Orders)
                }
            };

            List<object> cosmosDocs;
            cosmosDocs = await _mediator.Send(query, CancellationToken.None);

            if (cosmosDocs.Any())
            {
                return cosmosDocs.Select(doc => Json.DeserializeObject<OrderDoc>((JToken)doc)).ToList();
            }
            else
            {
                return new List<OrderDoc>();
            }
        }
    }
}
