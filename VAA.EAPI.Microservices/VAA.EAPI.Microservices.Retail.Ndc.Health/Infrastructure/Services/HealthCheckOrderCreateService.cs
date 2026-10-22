using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOrderCreateService : IHealthCheckOrderCreateService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOrderCreateService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOrderCreateView>> GetOrderCreateHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckordercreate.FromSqlRaw(ConfigReader.Get(Constants.Environment.OrderCreateHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
