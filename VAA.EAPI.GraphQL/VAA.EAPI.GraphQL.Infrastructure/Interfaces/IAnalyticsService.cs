
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IAnalyticsService
    {
        Task TrackAnalytics(TrackAnalyticsMessage trackAnalyticsMessage);
    }
}
