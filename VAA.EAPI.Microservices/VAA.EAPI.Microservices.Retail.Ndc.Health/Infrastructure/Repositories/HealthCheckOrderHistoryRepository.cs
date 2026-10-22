using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOrderHistoryRepository : IHealthCheckOrderHistoryRepository
    {
        private readonly IHealthCheckOrderHistoryService _healthCheckOrderHistoryService;

        public HealthCheckOrderHistoryRepository(IHealthCheckOrderHistoryService healthCheckOrderHistoryService)
        {
            _healthCheckOrderHistoryService = healthCheckOrderHistoryService;
        }

        public async Task<List<HealthCheckOrderHistoryView>> GetOrderHistoryHealthReport()
        {
            return await _healthCheckOrderHistoryService.GetOrderHistoryHealthReport();
        }
    }
}


