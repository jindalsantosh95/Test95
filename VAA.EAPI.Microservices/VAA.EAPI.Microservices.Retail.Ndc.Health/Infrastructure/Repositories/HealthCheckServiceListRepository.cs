using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckServiceListRepository : IHealthCheckServiceListRepository
    {
        private readonly IHealthCheckServiceListService _healthCheckServiceListService;

        public HealthCheckServiceListRepository(IHealthCheckServiceListService healthCheckServiceListService)
        {
            _healthCheckServiceListService = healthCheckServiceListService;
        }

        public async Task<List<HealthCheckServiceListView>> GetServiceListHealthReport()
        {
            return await _healthCheckServiceListService.GetServiceListHealthReport();
        }
    }
}