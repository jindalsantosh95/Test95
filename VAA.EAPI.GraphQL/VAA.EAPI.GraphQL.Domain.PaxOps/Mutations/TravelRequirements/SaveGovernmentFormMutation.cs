using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class SaveGovernmentFormMutation(
        ITravelRequirementsServiceProvider travelRequirementsServiceProvider,
        ITelemetryService telemetryService,
        ILogger<SaveGovernmentFormMutation> logger)
    {
        private readonly ITravelRequirementsServiceProvider _travelRequirementsServiceProvider = travelRequirementsServiceProvider
            ?? throw new ArgumentNullException(nameof(travelRequirementsServiceProvider));
        private readonly ILogger<SaveGovernmentFormMutation> _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITelemetryService _telemetryService = telemetryService
            ?? throw new ArgumentNullException(nameof(telemetryService));

        [GraphQLName("SaveGovernmentFormTravelRequirements")]
        [GraphQLDescription("Records completion of one client-selected Government Form for one passenger and slice, then returns refreshed Travel Requirements state.")]
        public async Task<TravelRequirementsResponse> SaveGovernmentFormTravelRequirements(
            string travelRequirementsId,
            string passengerId,
            string sliceId,
            string governmentFormId,
            string linkId,
            string text,
            string href,
            IResolverContext context)
        {
            var missingFields = new List<string>();
            if (travelRequirementsId.IsNullOrWhiteSpace()) missingFields.Add(nameof(travelRequirementsId));
            if (passengerId.IsNullOrWhiteSpace()) missingFields.Add(nameof(passengerId));
            if (sliceId.IsNullOrWhiteSpace()) missingFields.Add(nameof(sliceId));
            if (governmentFormId.IsNullOrWhiteSpace()) missingFields.Add(nameof(governmentFormId));
            if (linkId.IsNullOrWhiteSpace()) missingFields.Add(nameof(linkId));
            if (text.IsNullOrWhiteSpace()) missingFields.Add(nameof(text));
            if (href.IsNullOrWhiteSpace()) missingFields.Add(nameof(href));

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

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_GOVERNMENT_FORM_SUBMITTED, Data: await TelemetryHelper.TravelRequirementsGovernmentFormSubmitted(travelRequirementsId, passengerId, sliceId)));

                var serviceRequest = await Infrastructure.Models.TravelRequirements
                    .TravelRequirementsSaveGovernmentFormsAttestationServiceRequest.CreateRequestAsync(
                        travelRequirementsId,
                        passengerId,
                        sliceId,
                        governmentFormId,
                        linkId,
                        text,
                        href);
                var serviceResponse = await _travelRequirementsServiceProvider
                    .SaveGovernmentFormsAttestationAsync(serviceRequest);

                var targetSlice = serviceResponse?.Passengers?
                    .FirstOrDefault(passenger => passenger.PassengerId.Is(passengerId))?
                    .SliceRequirements?
                    .FirstOrDefault(slice => slice.SliceId.Is(sliceId));

                if (serviceResponse != null &&
                    !serviceResponse.RequirementsId.IsNullOrEmpty() &&
                    targetSlice?.GovernmentForms?.Any(form =>
                        form.Id.Is(governmentFormId) &&
                        form.Status == GovernmentFormsStatus.COMPLETED) == true)
                {
                    var response = await TravelRequirementsResponse.FromServiceResponse(serviceResponse);

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_GOVERNMENT_FORM_SAVED, Data: TelemetryHelper.TravelRequirementsGovernmentFormSaved(serviceResponse, passengerId, sliceId)));

                    return response;
                }

                throw new InvalidOperationException("Travel requirements response did not confirm Government Forms completion.");
            }
            catch (Exception ex)
            {
                return await TravelRequirementsFailureHandler.HandleAsync(
                    context,
                    ex,
                    new TravelRequirementsFailureContext
                    {
                        TravelRequirementsId = travelRequirementsId,
                        LoggingFailureContext = LoggingFailureContext.SAVE_GOVERNMENT_FORM,
                        Operation = "save Government Forms attestation",
                        DefaultMessage = TravelRequirementsErrorMessages.DefaultFailureMessage,
                        DefaultTitle = TravelRequirementsErrorMessages.DefaultFailureTitle
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
