using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOCNService : IHealthCheckOCNService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOCNService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOCNView>> GetOCNHealthReport()
        {
            return await _healthReportDbContext.VHealthcheckocn.FromSqlRaw(ConfigReader.Get(Constants.Environment.OCNHealthReportQuery)).ToListAsync();
        }
    }
}