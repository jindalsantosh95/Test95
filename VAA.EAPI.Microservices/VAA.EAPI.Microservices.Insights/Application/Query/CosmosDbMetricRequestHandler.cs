using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Application.Providers;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Query
{
    public sealed class CosmosDbMetricRequestHandler : IRequestHandler<CosmosDbMetricRequest, Result<Response<IEnumerable<CosmosDbMetricResult>>>>
    {
        private readonly IMetricsProvider metricsProvider;

        public CosmosDbMetricRequestHandler(IMetricsProvider metricsProvider)
        {
            this.metricsProvider = metricsProvider ?? throw new System.ArgumentNullException(nameof(metricsProvider));
        }

        public async Task<Result<Response<IEnumerable<CosmosDbMetricResult>>>> Handle(CosmosDbMetricRequest request, CancellationToken cancellationToken)
        {
            var metrics = await metricsProvider.FetchCosmosMetricsAsync(request.ResourceNames);

            return new Response<IEnumerable<CosmosDbMetricResult>>
            {
                Status = Status.OK,
                Value = metrics
            };
        }
    }
}
