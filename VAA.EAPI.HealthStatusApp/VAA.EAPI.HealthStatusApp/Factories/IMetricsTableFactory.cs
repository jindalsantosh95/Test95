using VAA.EAPI.HealthStatusApp.Models.ViewModels;

namespace VAA.EAPI.HealthStatusApp.Factories
{
    public interface IMetricsTableFactory
    {
        public Task<MetricsModel> GetMetricsTable();
        public Task<IEnumerable<DeploymentMetricsModel>> GetDeploymentMetrics();
        public Task<AppInsightsMetricsModel> GetHeimdallData(string timespan);
    }
}

