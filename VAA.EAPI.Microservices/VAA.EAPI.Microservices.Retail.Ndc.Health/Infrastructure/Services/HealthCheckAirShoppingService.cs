using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckAirShoppingService : IHealthCheckAirShoppingService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckAirShoppingService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckAirShoppingView>> GetAirShoppingHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckairshopping.FromSqlRaw(ConfigReader.Get(Constants.Environment.AirShoppingHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
