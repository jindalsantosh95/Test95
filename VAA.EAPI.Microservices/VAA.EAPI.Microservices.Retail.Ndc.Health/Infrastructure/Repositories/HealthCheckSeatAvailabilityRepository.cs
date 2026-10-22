using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckSeatAvailabilityRepository : IHealthCheckSeatAvailabilityRepository
    {
        private readonly IHealthCheckSeatAvailabilityService _healthCheckSeatAvailabilityService;

        public HealthCheckSeatAvailabilityRepository(IHealthCheckSeatAvailabilityService healthCheckSeatAvailabilityService)
        {
            _healthCheckSeatAvailabilityService = healthCheckSeatAvailabilityService;
        }

        public async Task<List<HealthCheckSeatAvailabilityView>> GetSeatAvailabilityHealthReport()
        {
           return await _healthCheckSeatAvailabilityService.GetSeatAvailabilityHealthReport();
        }
    }
}
