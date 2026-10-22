using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Repositories.Interfaces
{
    public interface IHealthCheckSeatAvailabilityRepository
    {
        Task<List<HealthCheckSeatAvailabilityView>> GetSeatAvailabilityHealthReport();
    }
}
