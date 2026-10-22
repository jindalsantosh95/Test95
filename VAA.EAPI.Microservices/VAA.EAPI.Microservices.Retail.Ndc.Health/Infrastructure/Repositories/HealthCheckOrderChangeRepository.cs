using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOrderChangeRepository : IHealthCheckOrderChangeRepository
    {
        private readonly IHealthCheckOrderChangeService _healthCheckOrderChangeService;

        public HealthCheckOrderChangeRepository(IHealthCheckOrderChangeService healthCheckOrderChangeService)
        {
            _healthCheckOrderChangeService = healthCheckOrderChangeService;
        }

        public async Task<List<HealthCheckOrderChangeView>> GetOrderChangeHealthReport()
        {
            return await _healthCheckOrderChangeService.GetOrderChangeHealthReport();
        }
    }
}

