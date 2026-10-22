using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SaveContactTracingMutation(ITravelRequirementsServiceProvider travelRequirementsServiceProvider, ITelemetryService telemetryService, ILogger<SaveContactTracingMutation> logger)
    {
        private readonly ITravelRequirementsServiceProvider _travelRequirementsServiceProvider = travelRequirementsServiceProvider ?? throw new ArgumentNullException(nameof(travelRequirementsServiceProvider));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        private readonly ILogger<SaveContactTracingMutation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("SaveContactTracingTravelRequirements")]
        [GraphQLDescription("Saves US destination address/contact tracing information for a passenger and returns refreshed Travel Requirements state.")]
        public async Task<TravelRequirementsResponse> SaveContactTracingTravelRequirements(string travelRequirementsId, string sliceId, IEnumerable<ContactTracing> contactTracing, IResolverContext context)
        {
            var contactTracingList = contactTracing?.ToList();
            var missingFields = new List<string>();

            if (travelRequirementsId.IsNullOrWhiteSpace()) missingFields.Add(nameof(travelRequirementsId));
            if (sliceId.IsNullOrWhiteSpace()) missingFields.Add(nameof(sliceId));
            if (contactTracingList?.Any() != true)
            {
                missingFields.Add(nameof(contactTracing));
            }
            else
            {
                foreach (var contactTracingItem in contactTracingList)
                {
                    if (contactTracingItem == null)
                    {
                        missingFields.Add(nameof(contactTracing));
                        continue;
                    }

                    if (contactTracingItem.PassengerId.IsNullOrWhiteSpace()) missingFields.Add($"{nameof(contactTracing)}.{nameof(contactTracingItem.PassengerId)}");
                    if (contactTracingItem.InTransit == false)
                    {
                        if (contactTracingItem.StreetAddress.IsNullOrWhiteSpace()) missingFields.Add($"{nameof(contactTracing)}.{nameof(contactTracingItem.StreetAddress)}");
                        if (contactTracingItem.City.IsNullOrWhiteSpace()) missingFields.Add($"{nameof(contactTracing)}.{nameof(contactTracingItem.City)}");
                        if (contactTracingItem.State.IsNullOrWhiteSpace()) missingFields.Add($"{nameof(contactTracing)}.{nameof(contactTracingItem.State)}");
                        if (contactTracingItem.Zip.IsNullOrWhiteSpace()) missingFields.Add($"{nameof(contactTracing)}.{nameof(contactTracingItem.Zip)}");
                    }

                    if (contactTracingItem.PrimaryPhoneNumber.IsNullOrWhiteSpace()) missingFields.Add($"{nameof(contactTracing)}.{nameof(contactTracingItem.PrimaryPhoneNumber)}");
                    if (contactTracingItem.PrimaryPhoneNumberCountryCode.IsNullOrWhiteSpace()) missingFields.Add($"{nameof(contactTracing)}.{nameof(contactTracingItem.PrimaryPhoneNumberCountryCode)}");
                }
            }

            missingFields = missingFields.Distinct().ToList();

            if (missingFields.Any())
            {
                context.ReportError($"Missing mandatory field(s): {string.Join(", ", missingFields)}");
                return null;
            }

            var passengerIds = contactTracingList.Select(x => x.PassengerId).ToList();

            try
            {
                travelRequirementsId = await TravelRequirementsIdHelper.ExtendSessions(_travelRequirementsServiceProvider, travelRequirementsId);
                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_CONTACT_TRACING_SUBMITTED, Data: await TelemetryHelper.TravelRequirementsContactTracingSubmitted(travelRequirementsId, contactTracingList)));

                var travelRequirementsResponse = await _travelRequirementsServiceProvider.SaveContactTracingAsync(await Infrastructure.Models.TravelRequirements.TravelRequirementsSaveContactTracingServiceRequest.CreateRequestAsync(travelRequirementsId, sliceId, contactTracingList));

                if (travelRequirementsResponse != null && !travelRequirementsResponse.RequirementsId.IsNullOrEmpty() && passengerIds.All(passengerId => travelRequirementsResponse.Passengers?.Any(x => x.PassengerId.Is(passengerId)) == true))
                {
                    var response = await TravelRequirementsResponse.FromServiceResponse(travelRequirementsResponse);

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_CONTACT_TRACING_SAVED, Data: TelemetryHelper.TravelRequirementsContactTracingSaved(travelRequirementsResponse, passengerIds)));

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
                        TravelRequirementsId = travelRequirementsId,
                        LoggingFailureContext = LoggingFailureContext.SAVE_CONTACT_TRACING,
                        Operation = "save contact tracing",
                        DefaultMessage = TravelRequirementsErrorMessages.DefaultFailureMessage,
                        DefaultTitle = TravelRequirementsErrorMessages.DefaultFailureTitle
                    },
                    _telemetryService,
                    _logger);
            }
        }
    }
}
