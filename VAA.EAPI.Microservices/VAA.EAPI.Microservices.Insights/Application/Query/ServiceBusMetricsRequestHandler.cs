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
    public sealed class ServiceBusMetricsRequestHandler : IRequestHandler<ServiceBusMetricsRequest, Result<Response<IEnumerable<ServiceBusMetricsResult>>>>
    {
        private readonly IMetricsProvider metricsProvider;

        public ServiceBusMetricsRequestHandler(IMetricsProvider metricsProvider)
        {
            this.metricsProvider = metricsProvider ?? throw new System.ArgumentNullException(nameof(metricsProvider));
        }

        public async Task<Result<Response<IEnumerable<ServiceBusMetricsResult>>>> Handle(ServiceBusMetricsRequest request, CancellationToken cancellationToken)
        {
            var metrics = await metricsProvider.FetchServiceBusMetricsAsync();

            return new Response<IEnumerable<ServiceBusMetricsResult>>
            {
                Status = Status.OK,
                Value = metrics
            };
        }
    }
}
