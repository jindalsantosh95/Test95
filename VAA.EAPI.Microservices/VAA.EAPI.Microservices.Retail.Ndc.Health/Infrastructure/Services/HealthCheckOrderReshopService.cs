using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOrderReshopService : IHealthCheckOrderReshopService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOrderReshopService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOrderReshopView>> GetOrderReshopHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckorderreshop.FromSqlRaw(ConfigReader.Get(Constants.Environment.OrderReshopHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
