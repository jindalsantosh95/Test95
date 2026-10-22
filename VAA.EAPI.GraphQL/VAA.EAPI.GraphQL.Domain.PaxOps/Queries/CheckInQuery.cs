using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CheckInQuery(ICheckInServiceProvider checkInQueryProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService)
    {
        private ICheckInServiceProvider _checkInQueryProvider = checkInQueryProvider ?? throw new ArgumentNullException(nameof(checkInQueryProvider));
        private readonly ILogAnalytics _logAnalytics = logAnalytics ?? throw new ArgumentNullException(nameof(logAnalytics));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));


        [GraphQLName("CheckIn")]
        [GraphQLDescription("Initiate a checkin session for a given booking")]
        public async Task<CheckInResponse> InitiateCheckIn(string pnrLocator, string eTicketNumber, string originCode, string givenNames, string surname, IResolverContext context)
        {
            var missingFields = new[]
            {
                (nameof(pnrLocator), pnrLocator),
                (nameof(givenNames), givenNames),
                (nameof(surname), surname),
                (nameof(originCode), originCode)
            }
           .Where(f => f.Item2.IsNullOrWhiteSpace())
           .Select(f => f.Item1)
           .ToList();

            if (missingFields.Count > 0)
            {
                context.ReportError(
                    string.Format(Application.MissingFieldMessage, string.Join(", ", missingFields))
                );
                return null;
            }

            _ = _logAnalytics.CheckInAnalyticsLogger(null, null, new CheckInLogDetails { Origin = originCode, PnrLocator = pnrLocator }, LoggingCheckinEvent.CHECKIN_SESSION_REQUESTED);
            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CHECKIN_SESSION_REQUESTED, Data: new Dictionary<string, object>()
            {
                { "PnrLocator", pnrLocator },
                { "OriginCode", originCode }
            }
            ));

            var msResponse = await _checkInQueryProvider.InitiateCheckInAsync(new CheckInRequest
            {
                PnrLocator = pnrLocator,
                ETicketNumber = eTicketNumber,
                OriginCode = originCode,
                GivenNames = givenNames,
                Surname = surname
            });

            if (msResponse?.OnlineCheckInStatus?.Available == true)
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CHECKIN_SESSION_CREATED, Data: await TelemetryHelper.CheckinSessionCreated(msResponse, originCode, pnrLocator)));

            return await CheckInResponse.CheckInResponseAsync(msResponse, null);
        }
    }
}
