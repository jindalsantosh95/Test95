
using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOrderHistoryService : IHealthCheckOrderHistoryService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOrderHistoryService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOrderHistoryView>> GetOrderHistoryHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckorderhistory.FromSqlRaw(ConfigReader.Get(Constants.Environment.OrderHistoryHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
