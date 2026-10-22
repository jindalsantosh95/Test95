using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOrderListService : IHealthCheckOrderListService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOrderListService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOrderListView>> GetOrderListHealthReport()
        {
            return await _healthReportDbContext.VHealthcheckorderlist.FromSqlRaw(ConfigReader.Get(Constants.Environment.OrderListHealthReportQuery)).ToListAsync();
        }
    }
}