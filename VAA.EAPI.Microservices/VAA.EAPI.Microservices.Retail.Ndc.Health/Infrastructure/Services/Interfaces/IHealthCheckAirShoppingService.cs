using VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Services.Interfaces
{
    public interface IHealthCheckAirShoppingService
    {
        Task<List<HealthCheckAirShoppingView>> GetAirShoppingHealthReport();
    }
}
