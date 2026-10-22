using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories
{
    public class HealthCheckOfferPriceRepository : IHealthCheckOfferPriceRepository
    {
        private readonly IHealthCheckOfferPriceService _healthCheckOfferPriceService;

        public HealthCheckOfferPriceRepository(IHealthCheckOfferPriceService healthCheckOfferPriceService)
        {
            _healthCheckOfferPriceService = healthCheckOfferPriceService;
        }

        public async Task<List<HealthCheckOfferPriceView>> GetOfferPriceHealthReport()
        {
           return await _healthCheckOfferPriceService.GetOfferPriceHealthReport();
        }
    }
}
