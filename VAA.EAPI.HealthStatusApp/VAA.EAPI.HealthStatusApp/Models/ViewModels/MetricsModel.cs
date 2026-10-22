using VAA.EAPI.HealthStatusApp.Models.MetricsModels;

namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class MetricsModel
    {
        public IEnumerable<RedisCacheMetricsModel> RedisCacheMetricsModels { get; set; }
        public IEnumerable<ServiceBusMetricsModel> ServiceBusMetricsModels { get; set; }
        public IEnumerable<ServicePlanMetricsModel> ServicePlanMetricsModels { get; set; }
        public IEnumerable<CosmosDbMetricsModel> CosmosDbMetricsModels { get; set; }
        public DotComModel DotComModel { get; set; }
        public IEnumerable<DeploymentMetricsModel> DeploymentMetricsModels { get; set; }
    }
}
