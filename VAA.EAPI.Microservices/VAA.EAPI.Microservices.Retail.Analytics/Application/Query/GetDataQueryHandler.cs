using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Analytics.Application.Query
{
    public sealed class GetDataQueryHandler : IRequestHandler<GetDataQuery, IEnumerable<TrackAnalyticsMessage>>
    {
        private readonly IMediator _mediator;

        public GetDataQueryHandler(IMediator mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        public async Task<IEnumerable<TrackAnalyticsMessage>> Handle(GetDataQuery request, CancellationToken cancellationToken)
        {
            CosmosReaderServiceQuery cosmosReaderServiceQuery = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.CosmosDatabaseEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.CosmosDatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.CosmosCollectionName),
                SqlQuery = $"SELECT * FROM {ConfigReader.Get(Constants.Environment.CosmosCollectionName)} c WHERE c.Date >= @From and c.Date <= @To",
                QueryParams = new() { new KeyValuePair<string, object>("@From", request.From), new KeyValuePair<string, object>("@To", request.To) }
            };

            return (await _mediator.Send(cosmosReaderServiceQuery, cancellationToken)).Select(x => (TrackAnalyticsMessage)(JObject)x);
        }
    }
}
