using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class LogAnalytics(IAnalyticsService analyticsService) : ILogAnalytics
    {
        private readonly IAnalyticsService _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));

        public async Task CheckInAnalyticsLogger(OnlineCheckInStatus? statusResponse, Enum? failureContext, CheckInLogDetails? checkInDetails, Enum subject)
        {
            var analyticsData = new Dictionary<string, object>()
                {
                    { "Title", statusResponse?.Title },
                    { "Code", statusResponse?.Code },
                    { "Message", statusResponse?.Message },
                    { "FailedOperation", failureContext?.ToString() },
                    { "PnrLocator", checkInDetails?.PnrLocator },
                    { "OriginCode", checkInDetails?.Origin }
                };

            await _analyticsService.TrackAnalytics((TrackAnalyticsMessage)(Type: Common.Constants.Application.TrackingAnalyticsType, Subject: subject?.ToString(), Data: analyticsData));
        }
    }
}
