using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOrderQuoteRepository : IHealthCheckOrderQuoteRepository
    {
        private readonly IHealthCheckOrderQuoteService _healthCheckOrderQuoteService;

        public HealthCheckOrderQuoteRepository(IHealthCheckOrderQuoteService healthCheckOrderQuoteService)
        {
            _healthCheckOrderQuoteService = healthCheckOrderQuoteService;
        }

        public async Task<List<HealthCheckOrderQuoteView>> GetOrderQuoteHealthReport()
        {
            return await _healthCheckOrderQuoteService.GetOrderQuoteHealthReport();
        }
    }
}
