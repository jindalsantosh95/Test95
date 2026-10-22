using VAA.EAPI.HealthStatusApp.Models.MetricsModels;

namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class DotComModel
    {
        public IEnumerable<ServicePlanMetricsModel> ServicePlanModels { get; set; }
        public IEnumerable<CosmosDbMetricsModel> CosmosDbModels { get; set; }
    }
}
