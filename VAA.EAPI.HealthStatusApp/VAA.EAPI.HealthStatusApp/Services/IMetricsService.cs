using VAA.EAPI.HealthStatusApp.Models;
using VAA.EAPI.HealthStatusApp.Models.MetricsModels;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public interface IMetricsService
    {
        public Task<IEnumerable<RedisCacheMetricsModel>> GetRedisCacheMetrics();
        public Task<IEnumerable<ServiceBusMetricsModel>> GetServiceBusMetrics();
        public Task<IEnumerable<DeploymentMetricsModel>> GetDeploymentMetrics();
        public Task<IEnumerable<ServicePlanMetricsModel>> GetServicePlanMetrics();
        public Task<IEnumerable<CosmosDbMetricsModel>> GetCosmosDbMetrics();
        public Task<LatestHeimdallResult> GetLatestHeimdallResult();
        public Task<AppInsightsMetricsModel> GetHeimdallDataAsync(string Timespan);
    }
}