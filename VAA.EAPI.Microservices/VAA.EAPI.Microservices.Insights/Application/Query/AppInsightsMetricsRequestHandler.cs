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
    public sealed class AppInsightsMetricsRequestHandler : IRequestHandler<AppInsightsMetricsRequest, Result<Response<IEnumerable<AppInsightsMetricsResult>>>>
    {
        private readonly IMetricsProvider metricsProvider;

        public AppInsightsMetricsRequestHandler(IMetricsProvider metricsProvider)
        {
            this.metricsProvider = metricsProvider ?? throw new System.ArgumentNullException(nameof(metricsProvider));
        }

        public async Task<Result<Response<IEnumerable<AppInsightsMetricsResult>>>> Handle(AppInsightsMetricsRequest request, CancellationToken cancellationToken)
        {
            var metrics = await metricsProvider.FetchAppInsightsMetricsAsync(request.AppId, request.ApiKey, request.Query);

            return new Response<IEnumerable<AppInsightsMetricsResult>>
            {
                Status = Status.OK,
                Value = metrics
            };
        }
    }
}
