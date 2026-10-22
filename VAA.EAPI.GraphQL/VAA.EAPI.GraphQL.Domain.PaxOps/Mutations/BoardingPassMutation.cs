using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class BoardingPassMutation
    {
        private ICheckInServiceProvider _checkInMutationProvider;
        private readonly ILogAnalytics _logAnalytics;
        private readonly ITelemetryService _telemetryService;
        public BoardingPassMutation(ICheckInServiceProvider checkInMutationProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService)
        {
            _checkInMutationProvider = checkInMutationProvider ?? throw new ArgumentNullException(nameof(checkInMutationProvider));
            _logAnalytics = logAnalytics ?? throw new ArgumentNullException(nameof(logAnalytics));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        [GraphQLName("GenerateBoardingPass")]
        [GraphQLDescription("Generates Boarding Pass with passenger and flight information including QR Code")]

        public async Task<BoardingPassResponse> GenerateBoardingPass(string checkInId, IResolverContext context)
        {
            BoardingPassResponse response;
            if (checkInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(checkInId)));
                return null;
            }

            try
            {
                checkInId = await CheckInIdHelper.ExtendSessions(_checkInMutationProvider, checkInId);

                string travelPartyId = await CheckInIdHelper.GetTravelPartyId(checkInId);
                string pnrLocator = await CheckInIdHelper.GetPnrLocator(checkInId);

                var serviceResponse = await _checkInMutationProvider.GenerateBoardingPassAsync((BoardingPassServiceRequest)(travelPartyId, pnrLocator));

                response= (BoardingPassResponse)serviceResponse;
            }
            catch (Exception)
            {
                context.ReportError(
                    ErrorBuilder.New()
                    .SetMessage(Application.OnlieCheckInFailDefaultMessage)
                    .SetExtension("title", Application.OnlieCheckInFailDefaultTiTle)
                    .SetExtension("code", "500")
                    .Build());
                response = null;
            }

            if (response?.BoardingPasses == null || response.BoardingPasses.Any() == false || response.BoardingPasses.All(x => x.QrCode == null))
            {
                _ = _logAnalytics.CheckInAnalyticsLogger(null, LoggingFailureContext.BOARDING_PASS_GENERATION, new CheckInLogDetails { Origin = CheckInIdHelper.GetOriginCode(checkInId).Result, PnrLocator = CheckInIdHelper.GetPnrLocator(checkInId).Result }, LoggingCheckinEvent.CHECKIN_END_REFUSED);
                var refusedData = await TelemetryHelper.CheckinEndRefused(null, null, null, response, null, checkInId, LoggingFailureContext.BOARDING_PASS_GENERATION);
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CHECKIN_END_REFUSED, Data: refusedData));
            }
            else
            {
                _ = _logAnalytics.CheckInAnalyticsLogger(null, null, new CheckInLogDetails { Origin = response.BoardingPasses.First().OriginAirportCode, PnrLocator = response.BoardingPasses.First().PnrLocator }, LoggingCheckinEvent.CHECKIN_END_SUCCESS);
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.AUTO_SEAT_ASSIGNED, Data: await TelemetryHelper.AutoSeatAssigned(checkInId, response)));
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.CHECKIN_END_SUCCESS, Data: await TelemetryHelper.CheckinEndSuccess(checkInId, response)));
            }

            return response;
        }
    }
}
