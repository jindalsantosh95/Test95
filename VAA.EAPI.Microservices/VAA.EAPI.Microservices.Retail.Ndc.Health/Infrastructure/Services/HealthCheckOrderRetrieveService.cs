using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOrderRetrieveService : IHealthCheckOrderRetrieveService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOrderRetrieveService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOrderRetrieveView>> GetOrderRetrieveHealthReport()
        {
            return await _healthReportDbContext.VHealthcheckorderretrieve.FromSqlRaw(ConfigReader.Get(Constants.Environment.OrderRetrieveHealthReportQuery)).ToListAsync();

        }
    }
}
