using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;


namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class TravelRequirementsQuery(ITravelRequirementsServiceProvider travelRequirementsServiceProvider, ITelemetryService telemetryService, ILogger<TravelRequirementsQuery> logger)
    {
        private ITravelRequirementsServiceProvider _travelRequirementsServiceProvider = travelRequirementsServiceProvider ?? throw new ArgumentNullException(nameof(travelRequirementsServiceProvider));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        private readonly ILogger<TravelRequirementsQuery> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("GetTravelRequirements")]
        [GraphQLDescription("Get Travel Requirements for all slices per passenger for a given booking")]
        public async Task<TravelRequirementsResponse> GetTravelRequirements(TravelRequirementsRequest request, IResolverContext context)
        {
            if (request ==  null)
            {
                return ErrorHelper.Fail<TravelRequirementsResponse>(context, "Mandatory field missing.", "Internal server error", "500");
            }

            var missingFields = new[]
            {
                (nameof(request.GivenNames), request.GivenNames),
                (nameof(request.Surname), request.Surname),
                (nameof(request.PnrLocator), request.PnrLocator),
            }
           .Where(f => f.Item2.IsNullOrWhiteSpace()).Select(f => f.Item1).ToList();

            if (missingFields.Count > 0)
            {
                context.ReportError(
                    string.Format(Application.MissingFieldMessage, string.Join(", ", missingFields))
                );

                return null;
            }

            _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_REQUESTED, Data: new Dictionary<string, object>()

            {
                { "PnrLocator", request.PnrLocator }
            }
            ));

            try
            {
                var msResponse = await _travelRequirementsServiceProvider.GetTravelRequirementsAsync(request);
                var response = await TravelRequirementsResponse.FromServiceResponse(msResponse);

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_CREATED, Data: TelemetryHelper.GetTravelRequirementsSessionCreated(msResponse)));

                return response;
            }

            catch (Exception ex)
            {
                return await TravelRequirementsFailureHandler.HandleAsync(
                    context,
                    ex,
                    new TravelRequirementsFailureContext
                    {
                        PnrLocator = request.PnrLocator,
                        LoggingFailureContext = LoggingFailureContext.GET_TRAVEL_REQUIREMENTS,
                        Operation = "retrieve travel requirements",
                        DefaultMessage = "PNR not found with given details.",
                        DefaultTitle = "Internal server error",
                        ReturnParsedError = false
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
