using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOfferPriceService : IHealthCheckOfferPriceService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOfferPriceService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOfferPriceView>> GetOfferPriceHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckofferprice.FromSqlRaw(ConfigReader.Get(Constants.Environment.OfferPriceHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
