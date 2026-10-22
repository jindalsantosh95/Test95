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
    public sealed class ServicePlanMetricRequestHandler : IRequestHandler<ServicePlanMetricRequest, Result<Response<IEnumerable<ServicePlanMetricResult>>>>
    {
        private readonly IMetricsProvider metricsProvider;

        public ServicePlanMetricRequestHandler(IMetricsProvider metricsProvider)
        {
            this.metricsProvider = metricsProvider ?? throw new System.ArgumentNullException(nameof(metricsProvider));
        }

        public async Task<Result<Response<IEnumerable<ServicePlanMetricResult>>>> Handle(ServicePlanMetricRequest request, CancellationToken cancellationToken)
        {
            var metrics = await metricsProvider.FetchServicePlanMetricsAsync();

            return new Response<IEnumerable<ServicePlanMetricResult>>
            {
                Status = Status.OK,
                Value = metrics
            };
        }
    }
}
