using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckServiceListService : IHealthCheckServiceListService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckServiceListService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckServiceListView>> GetServiceListHealthReport()
        {
            return await _healthReportDbContext.VHealthcheckservicelist.FromSqlRaw(ConfigReader.Get(Constants.Environment.ServiceListHealthReportQuery)).ToListAsync();
        }
    }
}