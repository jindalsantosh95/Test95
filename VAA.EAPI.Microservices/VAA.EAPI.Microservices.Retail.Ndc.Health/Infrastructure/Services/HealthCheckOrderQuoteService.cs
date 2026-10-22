using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckOrderQuoteService : IHealthCheckOrderQuoteService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckOrderQuoteService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckOrderQuoteView>> GetOrderQuoteHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckorderquote.FromSqlRaw(ConfigReader.Get(Constants.Environment.OrderQuoteHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
