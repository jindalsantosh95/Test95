using VAA.EAPI.HealthStatusApp.Models.ViewModels;
using VAA.EAPI.HealthStatusApp.Services;

namespace VAA.EAPI.HealthStatusApp.SignalRClasses
{
    public interface IHealthStatusClient
    {
        Task GetHomeDashboadStatus(IInsightsService insightsService);
        Task ReceiveHomeDashboadStatus(HomeModel homeModel);
        Task GetInsightsStatus(IInsightsService insightsService);
        Task ReceiveInitialHealthStatus(DashboardModel dashboardModel);
        Task GetAPIMStatusTable(IInsightsService insightsService);
        Task ReceiveAPIMStatusTable(IEnumerable<APIMStatusModel> apimTable);
        Task GetLinkedServicesStatusTable(IInsightsService insightsService);
        Task ReceiveLinkedServicesStatusTable(IEnumerable<LinkedServiceStatusModel> linkedServicesTable);
        Task GetMetrics(IInsightsService insightsService);
        Task ReceiveMetrics(MetricsModel linkedServicesTable);
        Task GetEnvDashboadStatus(IInsightsService insightsService);
        Task ReceiveEnvDashboadStatus(HomeModel homeModel);
    }

}
