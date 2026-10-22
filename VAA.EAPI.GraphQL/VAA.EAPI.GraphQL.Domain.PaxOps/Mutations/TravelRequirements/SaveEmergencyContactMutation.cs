using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SaveEmergencyContactMutation(ITravelRequirementsServiceProvider travelRequirementsServiceProvider, ITelemetryService telemetryService, ILogger<SaveEmergencyContactMutation> logger)
    {
        private const string Iso2CountryCodePattern = "^[A-Z]{2}$";

        private readonly ITravelRequirementsServiceProvider _travelRequirementsServiceProvider = travelRequirementsServiceProvider ?? throw new ArgumentNullException(nameof(travelRequirementsServiceProvider));
        private readonly ITelemetryService _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        private readonly ILogger<SaveEmergencyContactMutation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("SaveEmergencyContactTravelRequirements")]
        [GraphQLDescription("Saves optional emergency contact details for one or more passengers and returns refreshed Travel Requirements state.")]
        public async Task<TravelRequirementsResponse> SaveEmergencyContactTravelRequirements(string travelRequirementsId, bool applyToAllPassengers, IEnumerable<EmergencyContact> emergencyContacts, IResolverContext context)
        {
            var missingFields = new List<string>();
            var invalidFields = new List<string>();
            var submittedEmergencyContacts = emergencyContacts?.ToList() ?? [];

            if (travelRequirementsId.IsNullOrWhiteSpace()) missingFields.Add(nameof(travelRequirementsId));
            if (!submittedEmergencyContacts.Any()) missingFields.Add(nameof(emergencyContacts));
            if (applyToAllPassengers && submittedEmergencyContacts.Count > 1) invalidFields.Add("applyToAllPassengers cannot be true when multiple emergencyContacts are supplied");

            foreach (var emergencyContact in submittedEmergencyContacts)
            {
                if (emergencyContact is null)
                {
                    missingFields.Add(nameof(emergencyContacts));
                    continue;
                }

                if (emergencyContact.PassengerId.IsNullOrWhiteSpace()) missingFields.Add($"emergencyContacts.{nameof(emergencyContact.PassengerId)}");

                if (!emergencyContact.PassengerRefused)
                {
                    if (emergencyContact.FirstName.IsNullOrWhiteSpace()) missingFields.Add($"emergencyContacts.{nameof(emergencyContact.FirstName)}");
                    if (emergencyContact.LastName.IsNullOrWhiteSpace()) missingFields.Add($"emergencyContacts.{nameof(emergencyContact.LastName)}");
                    if (emergencyContact.PhoneCountryCode.IsNullOrWhiteSpace()) missingFields.Add($"emergencyContacts.{nameof(emergencyContact.PhoneCountryCode)}");
                    if (emergencyContact.PhoneNumber.IsNullOrWhiteSpace()) missingFields.Add($"emergencyContacts.{nameof(emergencyContact.PhoneNumber)}");
                    if (!emergencyContact.PhoneCountryCode.IsNullOrWhiteSpace() && !Regex.IsMatch(emergencyContact.PhoneCountryCode, Iso2CountryCodePattern)) invalidFields.Add($"emergencyContacts.{nameof(emergencyContact.PhoneCountryCode)} must be an ISO-2 country code");
                }
            }

            if (missingFields.Count > 0)
            {
                context.ReportError($"Missing mandatory field(s): {string.Join(", ", missingFields.Distinct())}");
                return null;
            }

            if (invalidFields.Count > 0)
            {
                context.ReportError($"Invalid field(s): {string.Join(", ", invalidFields.Distinct())}");
                return null;
            }

            try
            {
                travelRequirementsId = await TravelRequirementsIdHelper.ExtendSessions(_travelRequirementsServiceProvider, travelRequirementsId);
                var scopedPassengerIds = (await TravelRequirementsIdHelper.GetSelectedPassengers(travelRequirementsId) ?? []).ToList();

                if (applyToAllPassengers && !scopedPassengerIds.Any())
                {
                    context.ReportError("Invalid field(s): applyToAllPassengers requires a Travel Requirements passenger scope");
                    return null;
                }

                if (scopedPassengerIds.Any() && submittedEmergencyContacts.Any(contact => scopedPassengerIds.All(passengerId => !passengerId.Is(contact.PassengerId))))
                {
                    context.ReportError("Invalid field(s): emergencyContacts.PassengerId is not in the Travel Requirements passenger scope");
                    return null;
                }

                var emergencyContactsToSave = applyToAllPassengers
                    ? ExpandEmergencyContacts(submittedEmergencyContacts.First(), scopedPassengerIds).ToList()
                    : submittedEmergencyContacts;

                _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_EMERGENCY_CONTACT_SUBMITTED, Data: await TelemetryHelper.TravelRequirementsEmergencyContactSubmitted(travelRequirementsId, applyToAllPassengers, emergencyContactsToSave)));

                var travelRequirementsResponse = await _travelRequirementsServiceProvider.SaveEmergencyContactAsync(await SaveEmergencyContactServiceRequest.CreateRequestAsync(travelRequirementsId, emergencyContactsToSave));

                if (travelRequirementsResponse != null && !travelRequirementsResponse.RequirementsId.IsNullOrEmpty() && emergencyContactsToSave.Any() && emergencyContactsToSave.All(contact => travelRequirementsResponse.Passengers?.Any(x => x.PassengerId.Is(contact.PassengerId)) == true))
                {
                    var response = await TravelRequirementsResponse.FromServiceResponse(travelRequirementsResponse);

                    _ = _telemetryService.TrackTelemetry((TelemetryEvent)(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_EMERGENCY_CONTACT_SAVED, Data: TelemetryHelper.TravelRequirementsEmergencyContactSaved(travelRequirementsResponse, emergencyContactsToSave)));

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
                        LoggingFailureContext = LoggingFailureContext.SAVE_EMERGENCY_CONTACT,
                        Operation = "save emergency contact",
                        DefaultMessage = TravelRequirementsErrorMessages.DefaultFailureMessage,
                        DefaultTitle = TravelRequirementsErrorMessages.DefaultFailureTitle
                    },
                    _telemetryService,
                    _logger);
            }
        }

        private static IEnumerable<EmergencyContact> ExpandEmergencyContacts(EmergencyContact selectedEmergencyContact, IEnumerable<string> scopedPassengerIds)
        {
            foreach (var passengerId in scopedPassengerIds)
            {
                yield return new EmergencyContact
                {
                    PassengerId = passengerId,
                    FirstName = selectedEmergencyContact.FirstName,
                    LastName = selectedEmergencyContact.LastName,
                    PhoneCountryCode = selectedEmergencyContact.PhoneCountryCode,
                    PhoneNumber = selectedEmergencyContact.PhoneNumber,
                    PassengerRefused = selectedEmergencyContact.PassengerRefused
                };
            }
        }
    }
}
