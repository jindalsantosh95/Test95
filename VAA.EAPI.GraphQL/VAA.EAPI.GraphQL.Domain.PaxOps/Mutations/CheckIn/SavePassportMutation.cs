using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SavePassport;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SavePassportMutation
    {
        private ICheckInServiceProvider _checkInServiceProvider;
        private IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogAnalytics _logAnalytics;
        private readonly ITelemetryService _telemetryService;

        public SavePassportMutation(ICheckInServiceProvider checkInServiceProvider, IReferenceServiceProvider referenceServiceProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService)
        {
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logAnalytics = logAnalytics ?? throw new ArgumentNullException(nameof(logAnalytics));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        [GraphQLName("SavePassport")]
        [GraphQLDescription("Saves the Passport information for the given paasengers to initiate checkin process.")]
        public async Task<CheckInResponse> SavePassport(SavePassportRequest savePassportRequest, IResolverContext context)
        {
            if (savePassportRequest.CheckInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(savePassportRequest.CheckInId)));
                return null;
            }
            if (savePassportRequest.Document.DateOfBirth.IsNullOrEmpty() && savePassportRequest.Document.BirthDate == null)
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(savePassportRequest.Document.BirthDate)));
                return null;
            }
            if (savePassportRequest.Document.ExpirationDate.IsNullOrEmpty() && savePassportRequest.Document.ExpiryDate == null)
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(savePassportRequest.Document.ExpiryDate)));
                return null;
            }

            savePassportRequest.CheckInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, savePassportRequest?.CheckInId);

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.PASSPORT_DETAILS_SUBMITTED, Data: await TelemetryHelper.PassportDetailsSubmitted(savePassportRequest)));

            var checkInResponse = await _checkInServiceProvider.SavePassportAsync(await SavePassportServiceRequest.CreateRequestAsync(savePassportRequest), savePassportRequest.CheckInId);

            if (checkInResponse.CurrentSlice?.Destination != null && checkInResponse.Passengers != null && checkInResponse.Passengers.Any(x => x.Requirements?.ReturnDateStatus == Constants.ReturnDateStatus.REQUIRED))
                checkInResponse.CurrentSlice.Destination.CountryName = (await _referenceServiceProvider.GetCountryProfile())?.FirstOrDefault(x => x.ISOCountryCode.Is(checkInResponse?.CurrentSlice?.Destination?.CountryCode))?.CountryName;

            if (checkInResponse?.OnlineCheckInStatus?.Available == true)
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.PASSPORT_DETAILS_SAVED, Data: await TelemetryHelper.PassportDetailsSaved(checkInResponse, savePassportRequest.CheckInId)));

            return await CheckInResponse.CheckInResponseAsync(checkInResponse, savePassportRequest?.CheckInId);
        }
    }
}
