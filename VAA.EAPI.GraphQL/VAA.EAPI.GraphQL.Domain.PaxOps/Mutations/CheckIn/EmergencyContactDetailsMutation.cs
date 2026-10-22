using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class EmergencyContactDetailsMutation
    {
        private ICheckInServiceProvider _checkInServiceProvider;
        private readonly ILogAnalytics _logAnalytics;
        private readonly ITelemetryService _telemetryService;

        public EmergencyContactDetailsMutation(ICheckInServiceProvider checkInServiceProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService)
        {
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _logAnalytics = logAnalytics ?? throw new ArgumentNullException(nameof(logAnalytics));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        [GraphQLName("SaveEmergencyContactDetails")]
        [GraphQLDescription("Update passenger Emergency contact details during checkin process.")]
        public async Task<CheckInResponse> SaveEmergencyContactDetails(string checkInId, bool applyToAllPassengers, IEnumerable<EmergencyContact> emergencyContacts, IResolverContext context)
        {
            if (checkInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(checkInId)));
                return null;
            }

            checkInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, checkInId);

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.EMERGENCY_CONTACT_SUBMITTED, Data: await TelemetryHelper.EmergencyContactSubmitted(checkInId, applyToAllPassengers, emergencyContacts)));

            var response = await CheckInResponse.CheckInResponseAsync(await _checkInServiceProvider.SaveEmergencyContactDetails(await EmergencyContactServiceRequest.CreateRequestAsync(checkInId, applyToAllPassengers, emergencyContacts), checkInId), checkInId);

            if (response?.OnlineCheckInStatus?.Available == true)
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.EMERGENCY_CONTACT_SAVED, Data: await TelemetryHelper.EmergencyContactSaved(response, checkInId)));

            return response;
        }
    }
}
