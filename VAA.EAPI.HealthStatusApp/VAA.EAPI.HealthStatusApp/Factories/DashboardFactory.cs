using Azure;
using System.Reflection;
using VAA.EAPI.HealthStatusApp.Enums;
using VAA.EAPI.HealthStatusApp.Helpers;
using VAA.EAPI.HealthStatusApp.Models;
using VAA.EAPI.HealthStatusApp.Models.ViewModels;
using VAA.EAPI.HealthStatusApp.Services;

namespace VAA.EAPI.HealthStatusApp.Factories
{
    public class DashboardFactory : IDashboardFactory
    {
        private readonly IInsightsService _service;
        private readonly IAzureStatusFeedService _azureService;
        private readonly IMetricsService _metricsService; 
        private readonly IServiceNowService _nowService;
        private static IConfiguration _configuration;
        private readonly IInsightsConnector _insightsConnector;
        private readonly string _checkUrl;

        public DashboardFactory(IInsightsService service, IAzureStatusFeedService azureService, IMetricsService metricsService, IServiceNowService serviceNowService, IInsightsConnector insightsConnector, IConfiguration configuration)
        {
            _service = service;
            _azureService = azureService;
            _metricsService = metricsService;
            _nowService = serviceNowService;
            _configuration = configuration;
            _insightsConnector = insightsConnector;
            _checkUrl = _configuration["HEALTH_CHECK_URL"];
        }

        public async Task<DashboardModel> GetDashboadStatus()
        {
            var model = new DashboardModel();
            var response = await _insightsConnector.GetInsightsStatus();
            var score = await _azureService.CalculateAzureScore();
            var serviceNowDboRequests = await _nowService.GetServiceNowFeedAsync();
            model.ServiceNowViewModels = serviceNowDboRequests.Select(x => new ServiceNowViewModel
            {
                IncidentNumber = x.Number,
                Category = x.Category,
                Channel = x.Channel,
                Priority = x.Priority,
                ConfigurationItem = x.ConfigurationItem,
                Created = x.Created,
                Shortdescription = x.Shortdescription,
                State = x.State
            });

            model.HeimdallResult = await _metricsService.GetLatestHeimdallResult();
            var healthReport = await _service.GetInisghtsCatalogueForDashboard();

            if (healthReport is not null)
            {
                model.Pie = new PieScoresModel();
                model.Pie.FirstScore = healthReport.APIMScore;
                model.Pie.SecondScore = healthReport.UnhealthyScore;
                model.GraphQlServices = healthReport.GraphQlServices;
                model.ServiceBusServices = healthReport.ServiceBusServices;
                model.CosmosServices = healthReport.CosmosServices;
                model.DashboardIssues = healthReport.Issues;
            }

            if (!string.IsNullOrEmpty(_checkUrl) && !_checkUrl.Contains("Test"))
            {
                var healthReportUAT = await _service.GetInisghtsCatalogueForDashboard(InsightsEnvironment.UAT);

                if (healthReportUAT is not null)
                {
                    model.ShowUAT = true;
                    model.UATPie = new PieScoresModel
                    {
                        FirstScore = healthReportUAT.APIMScore + healthReport.GraphQlScore,
                        SecondScore = healthReportUAT.UnhealthyScore + healthReportUAT.UnhealthyGraphQlScore
                    };

                    model.DashboardUATIssues = healthReportUAT.Issues;
                }
            }

            model.AzureRssScore = score;

            if (response.Item1 != 2)
            {
                model.IsInsightsHealthy = false;
                model.BannerText = $"Insights Connection: {response.Item2}";
            }
            else
            {
                model.IsInsightsHealthy = true;
                model.BannerText = healthReport?.IssueAlert;
            }

            return model;
        }

        public async Task<HomeModel> GetHomeDashboadStatus()
        {
            var model = new HomeModel();
            var response = await _insightsConnector.GetInsightsStatus();
            var score = await _azureService.CalculateAzureScore();
            var healthReport = await _service.GetInisghtsCatalogueForDashboard();
            if (healthReport is not null)
            {
                model.ApimPie = new PieScoresModel();
                model.ApimPie.FirstScore = healthReport.APIMScore;
                model.ApimPie.SecondScore = healthReport.UnhealthyScore;
                model.GraphQlPie = new PieScoresModel();
                model.GraphQlPie.FirstScore = healthReport.GraphQlScore;
                model.GraphQlPie.SecondScore = healthReport.UnhealthyGraphQlScore;
                model.CosmosPie = new PieScoresModel();
                model.CosmosPie.FirstScore = healthReport.CosmosScore;
                model.CosmosPie.SecondScore = healthReport.UnhealthyCosmosScore;
                model.ServiceBusPie = new PieScoresModel();
                model.ServiceBusPie.FirstScore = healthReport.ServiceBusScore;
                model.ServiceBusPie.SecondScore = healthReport.UnhealthyServiceBusScore;
                model.DashboardIssues = healthReport.Issues;
            }

            var serviceNowDboRequests = await _nowService.GetServiceNowFeedAsync();
            model.ServiceNowViewModels = serviceNowDboRequests.Select(x => new ServiceNowViewModel
            {
                IncidentNumber = x.Number,
                Category = x.Category,
                Channel = x.Channel,
                Priority = x.Priority,
                ConfigurationItem = x.ConfigurationItem,
                Created = x.Created,
                Shortdescription = x.Shortdescription,
                State = x.State
            });

            model.AzureRssScore = score;

            if (response.Item1 != 2)
            {
                model.IsInsightsHealthy = false;
                model.BannerText = $"Insights Connection: {response.Item2}";
            }
            else
            {
                model.IsInsightsHealthy = true;
                model.BannerText = healthReport?.IssueAlert;
            }

            return model;
        }



        public async Task<HomeModel> GetEnvDashboadStatus(int env)
        {
            var model = new HomeModel();
            var healthReport = await _service.GetInisghtsCatalogueForDashboard((InsightsEnvironment)env);

            if (healthReport is not null)
            {
                model.ApimPie = new PieScoresModel();
                model.ApimPie.FirstScore = healthReport.APIMScore;
                model.ApimPie.SecondScore = healthReport.UnhealthyScore;
                model.GraphQlPie = new PieScoresModel();
                model.GraphQlPie.FirstScore = healthReport.GraphQlScore;
                model.GraphQlPie.SecondScore = healthReport.UnhealthyGraphQlScore;
                model.CosmosPie = new PieScoresModel();
                model.CosmosPie.FirstScore = healthReport.CosmosScore;
                model.CosmosPie.SecondScore = healthReport.UnhealthyCosmosScore;
                model.ServiceBusPie = new PieScoresModel();
                model.ServiceBusPie.FirstScore = healthReport.ServiceBusScore;
                model.ServiceBusPie.SecondScore = healthReport.UnhealthyServiceBusScore;
                model.DashboardIssues = healthReport.Issues;
                model.IsInsightsHealthy = true;
            }
            return model;
        }
    }
}
