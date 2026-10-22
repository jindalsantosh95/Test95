using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SelectPassengersMutation
    {
        private ICheckInServiceProvider _checkInServiceProvider;
        private readonly ILogAnalytics _logAnalytics;
        private readonly ITelemetryService _telemetryService;

        public SelectPassengersMutation(ICheckInServiceProvider checkInServiceProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService)
        {
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _logAnalytics = logAnalytics ?? throw new ArgumentNullException(nameof(logAnalytics));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        [GraphQLName("SelectPassengers")]
        [GraphQLDescription("Select paasenger/s to initiate checkin process.")]
        public async Task<CheckInResponse> SelectPassengers(string checkInId, IEnumerable<string> passengerIds, IResolverContext context)
        {
            if (checkInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(checkInId)));
                return null;
            }

            checkInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, checkInId);
            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CHECKIN_PASSENGERS_REQUESTED, Data: await TelemetryHelper.CheckinPassengersRequested(checkInId, passengerIds)));

            var response = await CheckInResponse.CheckInResponseAsync(await _checkInServiceProvider.SelectPassengersAsync(await Models.SelectPassenger.SelectPassengerServiceRequest.CreateRequestAsync(checkInId, passengerIds), checkInId), checkInId);

            if (response?.OnlineCheckInStatus?.Available == true)
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CHECKIN_PASSENGERS_SELECTED, Data: await TelemetryHelper.CheckinPassengersSelected(response, checkInId)));

            return response;
        }
    }
}
