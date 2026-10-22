using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{
   public interface IMetricsProvider
    {
        Task<IEnumerable<RedisMetricsResult>> FetchRedisCacheMetricsAsync();

        Task<IEnumerable<ServiceBusMetricsResult>> FetchServiceBusMetricsAsync();

        Task<IEnumerable<DeploymentMetricsResult>> FetchDeploymentMetricsAsync(string[] DefinitionIds);

        Task<IEnumerable<AppInsightsMetricsResult>> FetchAppInsightsMetricsAsync(string appId, string apiKey, string query);
        Task<IEnumerable<CosmosDbMetricResult>> FetchCosmosMetricsAsync(IEnumerable<string> resourceNames);
        Task<IEnumerable<ServicePlanMetricResult>> FetchServicePlanMetricsAsync();
    }
}
