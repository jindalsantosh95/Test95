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
    public sealed class DeploymentMetricRequestHandler : IRequestHandler<DeploymentMetricRequest, Result<Response<IEnumerable<DeploymentMetricsResult>>>>
    {
        private readonly IMetricsProvider metricsProvider;

        public DeploymentMetricRequestHandler(IMetricsProvider metricsProvider)
        {
            this.metricsProvider = metricsProvider ?? throw new System.ArgumentNullException(nameof(metricsProvider));
        }

        public async Task<Result<Response<IEnumerable<DeploymentMetricsResult>>>> Handle(DeploymentMetricRequest request, CancellationToken cancellationToken)
        {
            var result = await metricsProvider.FetchDeploymentMetricsAsync(request.DefinitionIds);

            return new Response<IEnumerable<DeploymentMetricsResult>>
            {
                Status = Status.OK,
                Value = result
            };
        }
    }
}
