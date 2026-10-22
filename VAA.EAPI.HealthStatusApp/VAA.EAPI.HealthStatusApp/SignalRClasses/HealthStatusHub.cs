using VAA.EAPI.HealthStatusApp.Factories;
using VAA.EAPI.HealthStatusApp.Models;
using VAA.EAPI.HealthStatusApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace VAA.EAPI.HealthStatusApp.SignalRClasses
{
    [AllowAnonymous]
    public class HealthStatusHub : Hub<IHealthStatusClient>
    {
        private readonly IDashboardFactory _dashboardFactory;
        private readonly IStatusTableFactory _statusTableFactory;
        private readonly IMetricsTableFactory _metricsTableFactory;

        public HealthStatusHub(IDashboardFactory dashboardFactory, IStatusTableFactory statusTableFactory, IMetricsTableFactory metricsTableFactory, IConfiguration configuration)
        {
            _dashboardFactory = dashboardFactory;
            _statusTableFactory = statusTableFactory;
            _metricsTableFactory = metricsTableFactory;
        }

        public async Task GetInsightsStatus()
        {
            var responseModel = await _dashboardFactory.GetDashboadStatus();

            await Clients.All.ReceiveInitialHealthStatus(responseModel);
        }
        
        public async Task GetHomeDashboadStatus()
        {
            var responseModel = await _dashboardFactory.GetHomeDashboadStatus();

            await Clients.All.ReceiveHomeDashboadStatus(responseModel);
            
        }

        public async Task GetEnvDashboadStatus(int envNumber)
        {
            var responseModel = await _dashboardFactory.GetEnvDashboadStatus(envNumber);

            await Clients.All.ReceiveEnvDashboadStatus(responseModel);
            
        }

        public async Task GetAPIMStatusTable()
        {
            var responseModel = await _statusTableFactory.GetAPIMStatusTable();

            await Clients.All.ReceiveAPIMStatusTable(responseModel.APIMTable);
        }

        public async Task GetLinkedServicesStatusTable()
        {
            var responseModel = await _statusTableFactory.GetLinkedServiceTable();

            await Clients.All.ReceiveLinkedServicesStatusTable(responseModel.LinkedServicesTable);
        }


        public async Task GetMetrics()
        {
            var responseModel = await _metricsTableFactory.GetMetricsTable();

            await Clients.All.ReceiveMetrics(responseModel);
        }
    }
}
