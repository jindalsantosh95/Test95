using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOrderListRepository : IHealthCheckOrderListRepository
    {
        private readonly IHealthCheckOrderListService _healthCheckOrderListService;

        public HealthCheckOrderListRepository(IHealthCheckOrderListService healthCheckOrderListService)
        {
            _healthCheckOrderListService = healthCheckOrderListService;
        }

        public async Task<List<HealthCheckOrderListView>> GetOrderListHealthReport()
        {
            return await _healthCheckOrderListService.GetOrderListHealthReport();
        }
    }
}