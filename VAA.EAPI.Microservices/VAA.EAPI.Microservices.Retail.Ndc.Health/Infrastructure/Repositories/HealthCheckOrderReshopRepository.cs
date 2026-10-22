using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOrderReshopRepository : IHealthCheckOrderReshopRepository
    {
        private readonly IHealthCheckOrderReshopService _healthCheckOrderReshopService;

        public HealthCheckOrderReshopRepository(IHealthCheckOrderReshopService healthCheckOrderReshopService)
        {
            _healthCheckOrderReshopService = healthCheckOrderReshopService;
        }

        public async Task<List<HealthCheckOrderReshopView>> GetOrderReshopHealthReport()
        {
            return await _healthCheckOrderReshopService.GetOrderReshopHealthReport();
        }
    }
}
