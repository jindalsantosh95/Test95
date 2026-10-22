
using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOrderChangeService : IHealthCheckOrderChangeService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOrderChangeService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOrderChangeView>> GetOrderChangeHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckorderchange.FromSqlRaw(ConfigReader.Get(Constants.Environment.OrderChangeHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
