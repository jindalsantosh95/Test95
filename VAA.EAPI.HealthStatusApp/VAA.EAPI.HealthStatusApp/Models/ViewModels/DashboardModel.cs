using VAA.EAPI.HealthStatusApp.Models.MetricsModels;

namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class DashboardModel
    {
        public string? BannerText { get; set; }
        public int AzureRssScore { get; set; }
        public bool IsInsightsHealthy { get; set; }
        public LatestHeimdallResult HeimdallResult { get; set; }
        public PieScoresModel Pie { get; set; }
        public Dictionary<string, bool> GraphQlServices { get; set; }
        public Dictionary<string, bool> ServiceBusServices { get; set; }
        public Dictionary<string, bool> CosmosServices { get; set; }
        public IEnumerable<DashboardIssue> DashboardIssues { get; set; }
        public bool ShowUAT { get; set; }
        public PieScoresModel UATPie { get; set; }
        public IEnumerable<DashboardIssue> DashboardUATIssues { get; set; }
        public IEnumerable<ServiceNowViewModel> ServiceNowViewModels { get; set; }

    }
}
