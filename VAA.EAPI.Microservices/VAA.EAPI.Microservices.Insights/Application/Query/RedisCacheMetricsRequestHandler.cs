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
    public sealed class RedisCacheMetricsRequestHandler : IRequestHandler<RedisCacheMetricsRequest, Result<Response<IEnumerable<RedisMetricsResult>>>>
    {
        private readonly IMetricsProvider metricsProvider;

        public RedisCacheMetricsRequestHandler(IMetricsProvider metricsProvider)
        {
            this.metricsProvider = metricsProvider ?? throw new System.ArgumentNullException(nameof(metricsProvider));
        }

        public async Task<Result<Response<IEnumerable<RedisMetricsResult>>>> Handle(RedisCacheMetricsRequest request, CancellationToken cancellationToken)
        {
            var metrics = await metricsProvider.FetchRedisCacheMetricsAsync();

            return new Response<IEnumerable<RedisMetricsResult>>
            {
                Status = Status.OK,
                Value = metrics
            };
        }
    }
}
