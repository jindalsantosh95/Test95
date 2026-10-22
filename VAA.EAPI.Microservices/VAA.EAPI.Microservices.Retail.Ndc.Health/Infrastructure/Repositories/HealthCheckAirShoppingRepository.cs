using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckAirShoppingRepository : IHealthCheckAirShoppingRepository
    {
        private readonly IHealthCheckAirShoppingService _healthCheckAirShoppingService;

        public HealthCheckAirShoppingRepository(IHealthCheckAirShoppingService healthCheckAirShoppingService)
        {
            _healthCheckAirShoppingService = healthCheckAirShoppingService;
        }

        public async Task<List<HealthCheckAirShoppingView>> GetAirShoppingHealthReport()
        {
           return await _healthCheckAirShoppingService.GetAirShoppingHealthReport();
        }
    }
}
