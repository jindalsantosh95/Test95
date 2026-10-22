using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOrderRetrieveRepository : IHealthCheckOrderRetrieveRepository
    {
        private readonly IHealthCheckOrderRetrieveService _healthCheckOrderRetrieveService;

        public HealthCheckOrderRetrieveRepository(IHealthCheckOrderRetrieveService healthCheckOrderRetrieveService)
        {
            _healthCheckOrderRetrieveService = healthCheckOrderRetrieveService;
        }

        public async Task<List<HealthCheckOrderRetrieveView>> GetOrderRetrieveHealthReport()
        {
            return await _healthCheckOrderRetrieveService.GetOrderRetrieveHealthReport();
        }
    }
}


