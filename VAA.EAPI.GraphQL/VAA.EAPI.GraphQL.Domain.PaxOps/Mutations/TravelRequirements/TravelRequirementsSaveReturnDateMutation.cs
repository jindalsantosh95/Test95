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
    public class TravelRequirementsSaveReturnDateMutation(ITravelRequirementsServiceProvider travelRequirementsServiceProvider, ITelemetryService telemetryService, ILogger<TravelRequirementsSaveReturnDateMutation> logger)
    {
        private readonly ITravelRequirementsServiceProvider _travelRequirementsServiceProvider = travelRequirementsServiceProvider ?? throw new ArgumentNullException(nameof(travelRequirementsServiceProvider));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        private readonly ILogger<TravelRequirementsSaveReturnDateMutation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("TravelRequirementsSaveReturnDate")]
        [GraphQLDescription("Saves return date information for a passenger and returns refreshed Travel Requirements state.")]
        public async Task<TravelRequirementsResponse> SaveReturnDate(TravelRequirementsSaveReturnDateRequest request, IResolverContext context)
        {
            if (request == null)
            {
                return ErrorHelper.Fail<TravelRequirementsResponse>(context, "Mandatory field missing.", "Internal server error", "500");
            }

            var missingFields = new List<string>();

            if (request.TravelRequirementsId.IsNullOrWhiteSpace()) missingFields.Add(nameof(request.TravelRequirementsId));
            if (request.PassengerId.IsNullOrWhiteSpace()) missingFields.Add(nameof(request.PassengerId));
            if (!request.NoReturnDate && !request.ReturnOn.HasValue) missingFields.Add(nameof(request.ReturnOn));

            if (missingFields.Any())
            {
                context.ReportError($"Missing mandatory field(s): {string.Join(", ", missingFields)}");
                return null;
            }

            try
            {
                request.TravelRequirementsId = await TravelRequirementsIdHelper.ExtendSessions(_travelRequirementsServiceProvider, request.TravelRequirementsId);
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_RETURN_DATE_SUBMITTED, Data: await TelemetryHelper.TravelRequirementsReturnDateSubmitted(request)));

                var travelRequirementsResponse = await _travelRequirementsServiceProvider.SaveReturnDateAsync(await Infrastructure.Models.TravelRequirements.TravelRequirementsSaveReturnDateServiceRequest.CreateRequestAsync(request));

                if (travelRequirementsResponse != null && !travelRequirementsResponse.RequirementsId.IsNullOrEmpty() && travelRequirementsResponse.Passengers?.Any(x => x.PassengerId.Is(request.PassengerId)) == true)
                {
                    var response = await TravelRequirementsResponse.FromServiceResponse(travelRequirementsResponse);

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_RETURN_DATE_SAVED, Data: TelemetryHelper.TravelRequirementsReturnDateSaved(travelRequirementsResponse, request.PassengerId)));

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
                        LoggingFailureContext = LoggingFailureContext.SAVE_RETURN_DATE,
                        Operation = "save return date",
                        DefaultMessage = TravelRequirementsErrorMessages.DefaultFailureMessage,
                        DefaultTitle = TravelRequirementsErrorMessages.DefaultFailureTitle
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
