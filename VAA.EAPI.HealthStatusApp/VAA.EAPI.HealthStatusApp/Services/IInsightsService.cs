using VAA.EAPI.HealthStatusApp.Enums;
using VAA.EAPI.HealthStatusApp.Models;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public interface IInsightsService
    {
        public Task<HealthCheckDashboardData> GetInisghtsCatalogueForDashboard(InsightsEnvironment env = InsightsEnvironment.Default);
        public Task<IEnumerable<APIMStatusModel>> GetAPIMStatusTableData();
        public Task<IEnumerable<LinkedServiceStatusModel>> GetLinkedServiceTableData();
    }
}
