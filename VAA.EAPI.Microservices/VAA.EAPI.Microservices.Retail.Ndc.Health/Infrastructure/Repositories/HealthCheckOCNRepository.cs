using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOCNRepository : IHealthCheckOCNRepository
    {
        private readonly IHealthCheckOCNService _healthCheckOCNService;

        public HealthCheckOCNRepository(IHealthCheckOCNService healthCheckOCNService)
        {
            _healthCheckOCNService = healthCheckOCNService;
        }

        public async Task<List<HealthCheckOCNView>> GetOCNHealthReport()
        {
            return await _healthCheckOCNService.GetOCNHealthReport();
        }
    }
}