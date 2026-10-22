using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOrderCreateRepository : IHealthCheckOrderCreateRepository
    {
        private readonly IHealthCheckOrderCreateService _healthCheckOrderCreateService;

        public HealthCheckOrderCreateRepository(IHealthCheckOrderCreateService healthCheckOrderCreateService)
        {
            _healthCheckOrderCreateService = healthCheckOrderCreateService;
        }

        public async Task<List<HealthCheckOrderCreateView>> GetOrderCreateHealthReport()
        {
           return await _healthCheckOrderCreateService.GetOrderCreateHealthReport();
        }
    }
}
