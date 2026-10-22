using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Interfaces
{
    public interface IHealthReportService
    {
        Task<HealthReportResponse> GetHealthReportStatus(HealthReportRequest request);
    }
}
