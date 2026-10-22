using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class SaveSecondaryDocumentMutation(
        ITravelRequirementsServiceProvider travelRequirementsServiceProvider,
        ITelemetryService telemetryService,
        ILogger<SaveSecondaryDocumentMutation> logger)
    {
        private readonly ITravelRequirementsServiceProvider _travelRequirementsServiceProvider = travelRequirementsServiceProvider
            ?? throw new ArgumentNullException(nameof(travelRequirementsServiceProvider));
        private readonly ILogger<SaveSecondaryDocumentMutation> _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITelemetryService _telemetryService = telemetryService
            ?? throw new ArgumentNullException(nameof(telemetryService));

        [GraphQLName("SaveSecondaryDocumentTravelRequirements")]
        [GraphQLDescription("Records customer attestation of a selected secondary travel document for one passenger and slice, then returns refreshed Travel Requirements state.")]
        public async Task<TravelRequirementsResponse> SaveSecondaryDocumentTravelRequirements(
            string travelRequirementsId,
            string passengerId,
            string sliceId,
            string secondaryDocumentId,
            string countryCode,
            IResolverContext context)
        {
            var missingFields = new List<string>();
            if (travelRequirementsId.IsNullOrWhiteSpace()) missingFields.Add(nameof(travelRequirementsId));
            if (passengerId.IsNullOrWhiteSpace()) missingFields.Add(nameof(passengerId));
            if (sliceId.IsNullOrWhiteSpace()) missingFields.Add(nameof(sliceId));
            if (secondaryDocumentId.IsNullOrWhiteSpace()) missingFields.Add(nameof(secondaryDocumentId));
            if (countryCode.IsNullOrWhiteSpace()) missingFields.Add(nameof(countryCode));

            if (missingFields.Count > 0)
            {
                context.ReportError($"Missing mandatory field(s): {string.Join(", ", missingFields)}");
                return null;
            }

            try
            {
                travelRequirementsId = await TravelRequirementsIdHelper.ExtendSessions(
                    _travelRequirementsServiceProvider,
                    travelRequirementsId);

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECONDARY_DOCUMENT_SUBMITTED, Data: await TelemetryHelper.TravelRequirementsSecondaryDocumentSubmitted(travelRequirementsId, passengerId, sliceId)));

                var serviceRequest = await Infrastructure.Models.TravelRequirements
                    .TravelRequirementsSaveSecondaryDocumentAttestationServiceRequest.CreateRequestAsync(
                        travelRequirementsId,
                        passengerId,
                        sliceId,
                        secondaryDocumentId,
                        countryCode);
                var serviceResponse = await _travelRequirementsServiceProvider
                    .SaveSecondaryDocumentAttestationAsync(serviceRequest);

                var targetSlice = serviceResponse?.Passengers?
                    .FirstOrDefault(passenger => passenger.PassengerId.Is(passengerId))?
                    .SliceRequirements?
                    .FirstOrDefault(slice => slice.SliceId.Is(sliceId));

                if (serviceResponse != null &&
                    !serviceResponse.RequirementsId.IsNullOrEmpty() &&
                    targetSlice?.SecondaryDocuments?.Any(document =>
                        document.Status == SecondaryDocumentStatus.COMPLETE &&
                        document.SecondaryDocumentApplicationOptions?.Any(application =>
                            application.SecondaryDocumentId.Is(secondaryDocumentId)) == true) == true)
                {
                    var response = await TravelRequirementsResponse.FromServiceResponse(serviceResponse);

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECONDARY_DOCUMENT_SAVED, Data: TelemetryHelper.TravelRequirementsSecondaryDocumentSaved(serviceResponse, passengerId, sliceId)));

                    return response;
                }

                throw new InvalidOperationException("Travel requirements response did not confirm secondary document completion.");
            }
            catch (Exception ex)
            {
                return await TravelRequirementsFailureHandler.HandleAsync(
                    context,
                    ex,
                    new TravelRequirementsFailureContext
                    {
                        TravelRequirementsId = travelRequirementsId,
                        LoggingFailureContext = LoggingFailureContext.SAVE_SECONDARY_DOCUMENT,
                        Operation = "save secondary document attestation",
                        DefaultMessage = TravelRequirementsErrorMessages.DefaultFailureMessage,
                        DefaultTitle = TravelRequirementsErrorMessages.DefaultFailureTitle
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
