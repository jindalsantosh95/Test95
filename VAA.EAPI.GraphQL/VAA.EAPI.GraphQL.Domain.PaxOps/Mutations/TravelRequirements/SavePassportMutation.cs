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
    public class SavePassportMutation(ITravelRequirementsServiceProvider travelRequirementsServiceProvider, ITelemetryService telemetryService, ILogger<SavePassportMutation> logger)
    {
        private ITravelRequirementsServiceProvider _travelRequirementsServiceProvider = travelRequirementsServiceProvider ?? throw new ArgumentNullException(nameof(travelRequirementsServiceProvider));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        private readonly ILogger<SavePassportMutation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("SavePassportTravelRequirements")]
        [GraphQLDescription("Saves the Passport information for the given passenger to initiate Travel Requirements process.")]
        public async Task<TravelRequirementsResponse> SavePassportTravelRequirements(TravelRequirementsSavePassportRequest request, IResolverContext context)
        {
            if (request == null)
            {
                return ErrorHelper.Fail<TravelRequirementsResponse>(context, "Mandatory field missing.", "Internal server error", "500");
            }

            var missingFields = new List<string>();

            if (request.TravelRequirementsId.IsNullOrWhiteSpace()) missingFields.Add(nameof(request.TravelRequirementsId));
            if (request.PassengerId.IsNullOrWhiteSpace()) missingFields.Add(nameof(request.PassengerId));
            if (request.Document == null) missingFields.Add(nameof(request.Document));

            if (missingFields.Count > 0)
            {
                context.ReportError($"Missing mandatory field(s): {string.Join(", ", missingFields)}");
                return null;
            }

            TravelRequirementsResponse response;

            try
            {
                request.TravelRequirementsId = await TravelRequirementsIdHelper.ExtendSessions(_travelRequirementsServiceProvider, request.TravelRequirementsId);
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_PASSPORT_DETAILS_SUBMITTED, Data: await TelemetryHelper.TravelRequirementsPassportDetailsSubmitted(request)));

                var travelRequirementsResponse = await _travelRequirementsServiceProvider.SavePassportAsync(await Infrastructure.Models.TravelRequirements.SavePassportServiceRequest.CreateRequestAsync(request));

                if (travelRequirementsResponse != null && !travelRequirementsResponse.RequirementsId.IsNullOrEmpty() && travelRequirementsResponse.Passengers?.Any(x => x.PassengerId.Is(request.PassengerId)) == true)
                {
                    
                    response = await TravelRequirementsResponse.FromServiceResponse(travelRequirementsResponse);

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_PASSPORT_DETAILS_SAVED, Data: TelemetryHelper.TravelRequirementsPassportDetailsSaved(travelRequirementsResponse, request.PassengerId)));

                    return response;
                }

                throw new InvalidOperationException("Travel requirements response validation failed.");
            }

            catch (Exception ex)
            {
                return await TravelRequirementsFailureHandler.HandleAsync(
                    context,
                    ex,
                    new TravelRequirementsFailureContext
                    {
                        TravelRequirementsId = request.TravelRequirementsId,
                        LoggingFailureContext = LoggingFailureContext.SAVE_PASSPORT,
                        Operation = "save passport details",
                        DefaultMessage = TravelRequirementsErrorMessages.DefaultFailureMessage,
                        DefaultTitle = TravelRequirementsErrorMessages.DefaultFailureTitle
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
