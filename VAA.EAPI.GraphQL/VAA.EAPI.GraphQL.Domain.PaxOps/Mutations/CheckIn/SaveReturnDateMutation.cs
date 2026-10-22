using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SaveReturnDate;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SaveReturnDateMutation
    {
        private ICheckInServiceProvider _checkInServiceProvider;
        private IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogAnalytics _logAnalytics;
        private readonly ITelemetryService _telemetryService;

        public SaveReturnDateMutation(ICheckInServiceProvider checkInServiceProvider, IReferenceServiceProvider referenceServiceProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService)
        {
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logAnalytics = logAnalytics ?? throw new ArgumentNullException(nameof(logAnalytics));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        [GraphQLName("SaveReturnDate")]
        [GraphQLDescription("Saves the Return date information for the given paasengers to initiate checkin process.")]
        public async Task<CheckInResponse> SaveReturnDate(SaveReturnDateRequest saveReturnDateRequest, IResolverContext context)
        {
            if (saveReturnDateRequest.CheckInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(saveReturnDateRequest.CheckInId)));
                return null;
            }

            saveReturnDateRequest.CheckInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, saveReturnDateRequest?.CheckInId);

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.RETURN_DATE_SUBMITTED, Data: await TelemetryHelper.ReturnDateSubmitted(saveReturnDateRequest)));

            var checkInResponse = await _checkInServiceProvider.SaveReturnDateAsync(await SaveReturnDateServiceRequest.CreateRequestAsync(saveReturnDateRequest), saveReturnDateRequest.CheckInId);

            if (checkInResponse?.OnlineCheckInStatus?.Available == false)
            {
                if (checkInResponse.OnlineCheckInStatus.Message.Contains("[country]"))
                {
                    var countryName = (await _referenceServiceProvider.GetCountryProfile())?.FirstOrDefault(x => x.ISOCountryCode.Is(checkInResponse?.CurrentSlice?.Destination?.CountryCode))?.CountryName ?? "your destination";
                    checkInResponse.OnlineCheckInStatus.Message.Replace("[country]", countryName);
                }
            }

            if (checkInResponse?.OnlineCheckInStatus?.Available == true)
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.RETURN_DATE_SAVED, Data: await TelemetryHelper.ReturnDateSaved(checkInResponse, saveReturnDateRequest.CheckInId)));

            return await CheckInResponse.CheckInResponseAsync(checkInResponse, saveReturnDateRequest?.CheckInId);
        }
    }
}
