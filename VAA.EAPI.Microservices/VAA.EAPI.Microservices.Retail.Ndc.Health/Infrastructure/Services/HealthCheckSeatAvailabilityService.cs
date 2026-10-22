using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services
{
    public class HealthCheckSeatAvailabilityService : IHealthCheckSeatAvailabilityService
    {
        private readonly HealthReportDbContext _healthReportDbContext;

        public HealthCheckSeatAvailabilityService(HealthReportDbContext healthReportDbContext)
        {
            _healthReportDbContext = healthReportDbContext;
        }

        public async Task<List<HealthCheckSeatAvailabilityView>> GetSeatAvailabilityHealthReport()
        {
            var result = await _healthReportDbContext.VHealthcheckseatavailability.FromSqlRaw(ConfigReader.Get(Constants.Environment.SeatAvailabilityHealthReportQuery)).ToListAsync();

            return result;
        }
    }
}
