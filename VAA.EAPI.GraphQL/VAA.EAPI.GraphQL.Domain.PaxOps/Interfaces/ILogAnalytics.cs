using VAA.EAPI.GraphQL.Domain.PaxOps.Models;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces
{
    public interface ILogAnalytics
    {
        Task CheckInAnalyticsLogger(OnlineCheckInStatus? statusResponse, Enum failureContext, CheckInLogDetails? successResponse, Enum subject);
    }
}
