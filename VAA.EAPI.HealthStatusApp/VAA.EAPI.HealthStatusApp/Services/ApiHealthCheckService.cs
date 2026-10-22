using Microsoft.AspNetCore.SignalR;
using VAA.EAPI.HealthStatusApp.Factories;
using VAA.EAPI.HealthStatusApp.SignalRClasses;

namespace VAA.EAPI.HealthStatusApp.Services
{
    public class ApiHealthCheckService : BackgroundService
    {
        private readonly IHubContext<HealthStatusHub, IHealthStatusClient> _hubContext; 
        private readonly ILogger<ApiHealthCheckService> _logger;
        private readonly IDashboardFactory _dashboardFactory;
        private readonly TimeSpan _delayBeforeStart;
        private readonly TimeSpan _interval;

        public ApiHealthCheckService(IHubContext<HealthStatusHub, IHealthStatusClient> hubContext, IDashboardFactory dashboardFactory, ILogger<ApiHealthCheckService> logger, TimeSpan delayBeforeStart, TimeSpan interval)
        {
            _hubContext = hubContext;
            _dashboardFactory = dashboardFactory;
            _logger = logger;
            _delayBeforeStart = delayBeforeStart;
            _interval = interval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            // Delay before the first execution
            await Task.Delay(_delayBeforeStart, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                GetHomeDashboard();
                GetTvDashboard();

                await Task.Delay(_interval, stoppingToken);
            }
        }


        private async Task GetHomeDashboard()
        {
            try
            {
                var model = await _dashboardFactory.GetHomeDashboadStatus();
                await _hubContext.Clients.All.ReceiveHomeDashboadStatus(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking health status for Function App: {ex.Message}");
            }
        }

        private async Task GetTvDashboard()
        {
            try
            {
                var model = await _dashboardFactory.GetDashboadStatus();
                await _hubContext.Clients.All.ReceiveInitialHealthStatus(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking health status for Function App: {ex.Message}");
            }
        }
    }
}
