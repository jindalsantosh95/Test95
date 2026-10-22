using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SaveContactTracingMutation
    {
        private ICheckInServiceProvider _checkInServiceProvider;
        private readonly ILogAnalytics _logAnalytics;
        private readonly ITelemetryService _telemetryService;

        public SaveContactTracingMutation(ICheckInServiceProvider checkInServiceProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService)
        {
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _logAnalytics = logAnalytics ?? throw new ArgumentNullException(nameof(logAnalytics));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        [GraphQLName("SaveContactTracing")]
        [GraphQLDescription("Update passenger contact tracing details for US inbound flights during checkin process.")]
        public async Task<CheckInResponse> SaveContactTracing(string checkInId, bool applyToAll, IEnumerable<ContactTracing> contactTracings, IResolverContext context)
        {
            if (checkInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(checkInId)));
                return null;
            }

            checkInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, checkInId);

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CONTACT_TRACING_SUBMITTED, Data: await TelemetryHelper.ContactTracingSubmitted(checkInId, applyToAll, contactTracings)));

            var response = await CheckInResponse.CheckInResponseAsync(await _checkInServiceProvider.SaveContactTracingAsync(await SaveContactTracingServiceRequest.CreateRequestAsync(checkInId, applyToAll, contactTracings), checkInId), checkInId);

            if (response?.OnlineCheckInStatus?.Available == true)
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CONTACT_TRACING_SAVED, Data: await TelemetryHelper.ContactTracingSaved(response, checkInId)));

            return response;
        }
    }
}
