using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class SaveEmergencyContactTravelRequirementsTests
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly SaveEmergencyContactMutation _mutation;
        private readonly TestTelemetryService _telemetryService;
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();

        public SaveEmergencyContactTravelRequirementsTests()
        {
            Setup.CreateTestEnvironmentSettings();
            _httpClientFactoryFixture = new HttpClientFactoryFixture();

            IHttpConnector httpConnector = new HttpConnector(NullLogger<HttpConnector>.Instance, _httpClientFactoryFixture.Current);
            ITravelRequirementsServiceProvider travelRequirementsServiceProvider = new TravelRequirementsServiceProvider(httpConnector);
            _telemetryService = new TestTelemetryService();

            _mutation = new SaveEmergencyContactMutation(
                travelRequirementsServiceProvider,
                _telemetryService,
                NullLogger<SaveEmergencyContactMutation>.Instance);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForMissingTravelRequirementsId()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveEmergencyContactTravelRequirements(null, false, [CreateEmergencyContact("01.01|resource-id")], context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Missing mandatory field(s): travelRequirementsId", error.Message);
            Assert.Empty(GetSaveEmergencyContactRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForMissingPassengerOnContact()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveEmergencyContactTravelRequirements(await CreateTravelRequirementsId(), false, [CreateEmergencyContact(null)], context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Missing mandatory field(s): emergencyContacts.PassengerId", error.Message);
            Assert.Empty(GetSaveEmergencyContactRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForMissingEmergencyContactDetails()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveEmergencyContactTravelRequirements(await CreateTravelRequirementsId(), false, [new EmergencyContact { PassengerId = "01.01|resource-id" }], context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Missing mandatory field(s): emergencyContacts.FirstName, emergencyContacts.LastName, emergencyContacts.PhoneCountryCode, emergencyContacts.PhoneNumber", error.Message);
            Assert.Empty(GetSaveEmergencyContactRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForNonIso2PhoneCountryCode()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveEmergencyContactTravelRequirements(await CreateTravelRequirementsId(), false, [CreateEmergencyContact("01.01|resource-id", phoneCountryCode: "44")], context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Invalid field(s): emergencyContacts.PhoneCountryCode must be an ISO-2 country code", error.Message);
            Assert.Empty(GetSaveEmergencyContactRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForApplyToAllWithMultipleContacts()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveEmergencyContactTravelRequirements(
                await CreateTravelRequirementsId(),
                true,
                [
                    CreateEmergencyContact("01.01|resource-id"),
                    CreateEmergencyContact("02.01|other-resource-id")
                ],
                context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Invalid field(s): applyToAllPassengers cannot be true when multiple emergencyContacts are supplied", error.Message);
            Assert.Empty(GetSaveEmergencyContactRequestPayloads());
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullAndReportErrorWithoutCallingDependency_ForApplyToAllWithoutPassengerScope()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveEmergencyContactTravelRequirements(
                await CreateTravelRequirementsIdWithoutPassengerScope(),
                true,
                [CreateEmergencyContact("01.01|resource-id")],
                context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Invalid field(s): applyToAllPassengers requires a Travel Requirements passenger scope", error.Message);
            Assert.Empty(GetSaveEmergencyContactRequestPayloads());
        }


        [Fact]
        public async Task Mutation_ShouldReturnTravelRequirementsResponseAndSendEmergencyContactPayload_ForGoodRequest()
        {
            var passengerId = "01.01|resource-id";
            var travelRequirementsId = await CreateTravelRequirementsId();

            var result = await _mutation.SaveEmergencyContactTravelRequirements(travelRequirementsId, false, [CreateEmergencyContact(passengerId)], null);

            Assert.NotNull(result);
            Assert.IsType<TravelRequirementsResponse>(result);
            Assert.True(JwtSecurityTokenHandler.CanReadToken(result.TravelRequirementsId));
            Assert.Equal("requirements-id", await TravelRequirementsIdHelper.GetRequirementsId(result.TravelRequirementsId));
            Assert.Equal(2, _telemetryService.Events.Count);
            var submittedTelemetry = _telemetryService.Events[0];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_EMERGENCY_CONTACT_SUBMITTED, submittedTelemetry.Type);
            Assert.Equal("CNMLQN", submittedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("requirements-id", submittedTelemetry.Data["RequirementsId"]?.ToString());
            Assert.Equal("False", submittedTelemetry.Data["ApplyToAllPassengers"]?.ToString());
            Assert.Equal($"[\"{passengerId}\"]", submittedTelemetry.Data["PassengerIds"]?.ToString());

            var savedTelemetry = _telemetryService.Events[1];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_EMERGENCY_CONTACT_SAVED, savedTelemetry.Type);
            Assert.Equal("CNMLQN", savedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("requirements-id", savedTelemetry.Data["RequirementsId"]?.ToString());
            Assert.Equal("True", savedTelemetry.Data["EmergencyContactPresent"]?.ToString());
            Assert.Equal("False", savedTelemetry.Data["ApplyToAllPassengers"]?.ToString());

            Assert.Equal(2, result.Passengers.Count());
            Assert.Equal(2, result.Slices.Count());

            var editedPassenger = result.Passengers.First(p => p.PassengerId.Is(passengerId));
            Assert.NotNull(editedPassenger.EmergencyContact);
            Assert.Equal("Jane", editedPassenger.EmergencyContact.FirstName);
            Assert.Equal("Helper", editedPassenger.EmergencyContact.LastName);
            Assert.Equal("GB", editedPassenger.EmergencyContact.PhoneCountryCode);
            Assert.Equal("7700900000", editedPassenger.EmergencyContact.PhoneNumber);
            Assert.False(editedPassenger.EmergencyContact.PassengerRefused);
            Assert.Equal(2, editedPassenger.SliceRequirements.Count());
            Assert.All(editedPassenger.SliceRequirements, requirement =>
            {
                Assert.True(requirement.PassportProvided);
                Assert.Equal(OverallCheckInStatus.TO_COMPLETE, requirement.Overall);
                Assert.Equal(ReturnDateStatus.REQUIRED, requirement.ReturnDateStatus);
                Assert.Equal(SecondaryDocumentStatus.REQUIRED, requirement.SecondaryDocumentStatus);
            });

            var passportPendingPassenger = result.Passengers.First(p => p.PassengerId.Is("02.01|other-resource-id"));
            Assert.Null(passportPendingPassenger.EmergencyContact);
            Assert.Equal(2, passportPendingPassenger.SliceRequirements.Count());
            Assert.All(passportPendingPassenger.SliceRequirements, requirement =>
            {
                Assert.False(requirement.PassportProvided);
                Assert.False(requirement.PassportVerified);
                Assert.Equal(ReturnDateStatus.PROVIDE_PASSPORT_FIRST, requirement.ReturnDateStatus);
                Assert.Equal(SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST, requirement.SecondaryDocumentStatus);
            });

            var payload = await GetSaveEmergencyContactRequestPayload(passengerId);
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
            Assert.Contains("\"firstName\":\"Jane\"", payload);
            Assert.Contains("\"lastName\":\"Helper\"", payload);
            Assert.Contains("\"phoneCountryCode\":\"GB\"", payload);
            Assert.Contains("\"phoneNumber\":\"7700900000\"", payload);
            Assert.DoesNotContain("\"passengerRefused\":true", payload);
        }

        [Fact]
        public async Task Mutation_ShouldTrackFailedTelemetry_WhenDependencyFails()
        {
            const string passengerId = "01.01|unmatched-emergency-contact-resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveEmergencyContactTravelRequirements(
                travelRequirementsId, false, [CreateEmergencyContact(passengerId)], context);

            Assert.Null(result);
            Assert.Single(resolverContext.Errors);
            Assert.Equal(2, _telemetryService.Events.Count);
            var failedTelemetry = _telemetryService.Events[1];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED, failedTelemetry.Type);
            Assert.Equal("CNMLQN", failedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("FAILED", failedTelemetry.Data["Status"]?.ToString());
            Assert.Equal(LoggingFailureContext.SAVE_EMERGENCY_CONTACT.ToString(), failedTelemetry.Data["TravelRequirementsStage"]?.ToString());
        }

        [Fact]
        public async Task Mutation_ShouldReturnTravelRequirementsResponseAndSendSingleEmergencyContactPayload_ForMultipleContacts()
        {
            var firstPassengerId = "01.01|resource-id";
            var secondPassengerId = "02.01|other-resource-id";
            var travelRequirementsId = await CreateTravelRequirementsId();

            var result = await _mutation.SaveEmergencyContactTravelRequirements(
                travelRequirementsId,
                false,
                [
                    CreateEmergencyContact(firstPassengerId, firstName: "Morgan", lastName: "Primary", phoneCountryCode: "GB", phoneNumber: "7700900001"),
                    CreateEmergencyContact(secondPassengerId, firstName: "Alex", lastName: "Backup", phoneCountryCode: "US", phoneNumber: "4045550100")
                ],
                null);

            Assert.NotNull(result);
            Assert.Equal("requirements-id", await TravelRequirementsIdHelper.GetRequirementsId(result.TravelRequirementsId));
            Assert.Equal(2, result.Passengers.Count());

            var firstPassenger = result.Passengers.Single(passenger => passenger.PassengerId.Is(firstPassengerId));
            var secondPassenger = result.Passengers.Single(passenger => passenger.PassengerId.Is(secondPassengerId));

            Assert.NotNull(firstPassenger.EmergencyContact);
            Assert.Equal("Morgan", firstPassenger.EmergencyContact.FirstName);
            Assert.Equal("Primary", firstPassenger.EmergencyContact.LastName);
            Assert.Equal("GB", firstPassenger.EmergencyContact.PhoneCountryCode);
            Assert.Equal("7700900001", firstPassenger.EmergencyContact.PhoneNumber);

            Assert.NotNull(secondPassenger.EmergencyContact);
            Assert.Equal("Alex", secondPassenger.EmergencyContact.FirstName);
            Assert.Equal("Backup", secondPassenger.EmergencyContact.LastName);
            Assert.Equal("US", secondPassenger.EmergencyContact.PhoneCountryCode);
            Assert.Equal("4045550100", secondPassenger.EmergencyContact.PhoneNumber);

            var payload = await GetSingleSaveEmergencyContactRequestPayload();
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.DoesNotContain("applyToAllPassengers", payload);
            Assert.Contains($"\"passengerId\":\"{firstPassengerId}\"", payload);
            Assert.Contains("\"firstName\":\"Morgan\"", payload);
            Assert.Contains("\"lastName\":\"Primary\"", payload);
            Assert.Contains("\"phoneNumber\":\"7700900001\"", payload);
            Assert.Contains($"\"passengerId\":\"{secondPassengerId}\"", payload);
            Assert.Contains("\"firstName\":\"Alex\"", payload);
            Assert.Contains("\"lastName\":\"Backup\"", payload);
            Assert.Contains("\"phoneNumber\":\"4045550100\"", payload);
        }

        [Fact]
        public async Task Mutation_ShouldApplySubmittedEmergencyContactToAllPassengersInTravelRequirementsScope()
        {
            var firstPassengerId = "01.01|resource-id";
            var secondPassengerId = "02.01|other-resource-id";
            var travelRequirementsId = await CreateTravelRequirementsId();

            var result = await _mutation.SaveEmergencyContactTravelRequirements(
                travelRequirementsId,
                true,
                [CreateEmergencyContact(firstPassengerId, firstName: "Jamie")],
                null);

            Assert.NotNull(result);
            Assert.Equal("requirements-id", await TravelRequirementsIdHelper.GetRequirementsId(result.TravelRequirementsId));
            Assert.Equal(2, result.Passengers.Count());
            Assert.Equal(2, result.Slices.Count());
            var savedTelemetry = Assert.Single(_telemetryService.Events.Where(telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_EMERGENCY_CONTACT_SAVED)));
            Assert.Equal("True", savedTelemetry.Data["EmergencyContactPresent"]?.ToString());

            var firstPassenger = result.Passengers.Single(passenger => passenger.PassengerId.Is(firstPassengerId));
            var secondPassenger = result.Passengers.Single(passenger => passenger.PassengerId.Is(secondPassengerId));

            Assert.NotNull(firstPassenger.EmergencyContact);
            Assert.Equal("Jamie", firstPassenger.EmergencyContact.FirstName);
            Assert.Equal("Helper", firstPassenger.EmergencyContact.LastName);
            Assert.Equal("GB", firstPassenger.EmergencyContact.PhoneCountryCode);
            Assert.Equal("7700900000", firstPassenger.EmergencyContact.PhoneNumber);

            Assert.NotNull(secondPassenger.EmergencyContact);
            Assert.Equal("Jamie", secondPassenger.EmergencyContact.FirstName);
            Assert.Equal("Helper", secondPassenger.EmergencyContact.LastName);
            Assert.Equal("GB", secondPassenger.EmergencyContact.PhoneCountryCode);
            Assert.Equal("7700900000", secondPassenger.EmergencyContact.PhoneNumber);

            Assert.NotNull(secondPassenger.SliceRequirements);
            var secondPassengerSliceRequirements = secondPassenger.SliceRequirements;
            Assert.All(secondPassengerSliceRequirements, requirement =>
            {
                Assert.False(requirement.PassportProvided);
                Assert.False(requirement.PassportVerified);
                Assert.Equal(ReturnDateStatus.PROVIDE_PASSPORT_FIRST, requirement.ReturnDateStatus);
                Assert.Equal(SecondaryDocumentStatus.PROVIDE_PASSPORT_FIRST, requirement.SecondaryDocumentStatus);
            });

            var payload = await GetSingleSaveEmergencyContactRequestPayload();
            Assert.DoesNotContain("applyToAllPassengers", payload);

            var serviceRequest = Json.Deserialize<SaveEmergencyContactServiceRequest>(payload);
            Assert.NotNull(serviceRequest);
            Assert.Equal("requirements-id", serviceRequest.RequirementsId);

            var emergencyContacts = serviceRequest.EmergencyContacts.ToList();
            Assert.Equal(2, emergencyContacts.Count);

            var firstPassengerContact = emergencyContacts.Single(contact => contact.PassengerId.Is(firstPassengerId));
            Assert.Equal("Jamie", firstPassengerContact.FirstName);
            Assert.Equal("Helper", firstPassengerContact.LastName);
            Assert.Equal("GB", firstPassengerContact.PhoneCountryCode);
            Assert.Equal("7700900000", firstPassengerContact.PhoneNumber);
            Assert.False(firstPassengerContact.PassengerRefused);

            var secondPassengerContact = emergencyContacts.Single(contact => contact.PassengerId.Is(secondPassengerId));
            Assert.Equal("Jamie", secondPassengerContact.FirstName);
            Assert.Equal("Helper", secondPassengerContact.LastName);
            Assert.Equal("GB", secondPassengerContact.PhoneCountryCode);
            Assert.Equal("7700900000", secondPassengerContact.PhoneNumber);
            Assert.False(secondPassengerContact.PassengerRefused);
        }

        private static Task<string> CreateTravelRequirementsId() =>
            TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null,
                "requirements-id",
                true,
                "TRAVEL",
                "TEST",
                "CNMLQN",
                ["01.01|resource-id", "02.01|other-resource-id"]);

        private static Task<string> CreateTravelRequirementsIdWithoutPassengerScope() =>
            TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null,
                "requirements-id",
                true,
                "TRAVEL",
                "TEST",
                "CNMLQN");

        private static EmergencyContact CreateEmergencyContact(string passengerId, string firstName = "Jane", string lastName = "Helper", string phoneCountryCode = "GB", string phoneNumber = "7700900000") => new()
        {
            PassengerId = passengerId,
            FirstName = firstName,
            LastName = lastName,
            PhoneCountryCode = phoneCountryCode,
            PhoneNumber = phoneNumber,
            PassengerRefused = false
        };

        private async Task<string> GetSingleSaveEmergencyContactRequestPayload()
        {
            var request = Assert.Single(GetSaveEmergencyContactRequestPayloads());

            return await request.Content!.ReadAsStringAsync(CancellationToken.None);
        }

        private async Task<string> GetSaveEmergencyContactRequestPayload(string passengerId)
        {
            var payloads = new List<string>();

            foreach (var request in GetSaveEmergencyContactRequestPayloads())
            {
                payloads.Add(await request.Content!.ReadAsStringAsync(CancellationToken.None));
            }

            return payloads
                .Single(payload => payload.Contains($"\"passengerId\":\"{passengerId}\"", StringComparison.Ordinal));
        }

        private IEnumerable<HttpRequestMessage> GetSaveEmergencyContactRequestPayloads()
        {
            return _httpClientFactoryFixture.Current
                .GetResponses($"{ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveEmergencyContact")
                .Select(response => response.RequestMessage)
                .Where(request => request?.RequestUri?.AbsolutePath.EndsWith("/TravelRequirements/SaveEmergencyContact", StringComparison.OrdinalIgnoreCase) == true)
                .Select(request => request!);
        }

    }
}
