namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class HomeModel
    {
        public string? BannerText { get; set; }
        public int AzureRssScore { get; set; }
        public bool IsInsightsHealthy { get; set; }
        public PieScoresModel ApimPie { get; set; }
        public PieScoresModel GraphQlPie { get; set; }
        public PieScoresModel CosmosPie { get; set; }
        public PieScoresModel ServiceBusPie { get; set; }
        public IEnumerable<DashboardIssue> DashboardIssues { get; set; }
        public IEnumerable<ServiceNowViewModel> ServiceNowViewModels { get; set; }

    }
}
