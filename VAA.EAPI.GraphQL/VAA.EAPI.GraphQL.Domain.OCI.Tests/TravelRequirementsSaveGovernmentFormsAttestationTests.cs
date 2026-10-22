using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public sealed class SaveGovernmentFormTravelRequirementsTests
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly SaveGovernmentFormMutation _mutation;
        private readonly TestTelemetryService _telemetryService;
        private static readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

        public SaveGovernmentFormTravelRequirementsTests()
        {
            Setup.CreateTestEnvironmentSettings();
            _httpClientFactoryFixture = new HttpClientFactoryFixture();

            IHttpConnector connector = new HttpConnector(NullLogger<HttpConnector>.Instance, _httpClientFactoryFixture.Current);
            ITravelRequirementsServiceProvider provider = new TravelRequirementsServiceProvider(connector);
            _telemetryService = new TestTelemetryService();

            _mutation = new SaveGovernmentFormMutation(
                provider,
                _telemetryService,
                NullLogger<SaveGovernmentFormMutation>.Instance);
        }

        [Fact]
        public async Task Mutation_ShouldReturnCompletedRefreshedState_ForTargetPassengerAndSlice()
        {
            const string passengerId = "01.01|government-forms-resource-id";
            const string sliceId = "slice-government-forms";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);

            var result = await _mutation.SaveGovernmentFormTravelRequirements(
                travelRequirementsId,
                passengerId,
                sliceId,
                "32738f95-5023-4a23-bf8c-16dd388cbaff",
                "Maldives Traveller Declaration",
                "Maldives Traveller Declaration",
                "https://imuga.immigration.gov.mv/",
                null);

            Assert.True(_jwtSecurityTokenHandler.CanReadToken(result.TravelRequirementsId));
            Assert.Equal(2, _telemetryService.Events.Count);
            var submittedTelemetry = _telemetryService.Events[0];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_GOVERNMENT_FORM_SUBMITTED, submittedTelemetry.Type);
            Assert.Equal("CNMLQN", submittedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("requirements-id", submittedTelemetry.Data["RequirementsId"]?.ToString());
            Assert.Equal(passengerId, submittedTelemetry.Data["PassengerId"]?.ToString());
            Assert.Equal(sliceId, submittedTelemetry.Data["SliceId"]?.ToString());

            var savedTelemetry = _telemetryService.Events[1];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_GOVERNMENT_FORM_SAVED, savedTelemetry.Type);
            Assert.Equal("CNMLQN", savedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("requirements-id", savedTelemetry.Data["RequirementsId"]?.ToString());
            Assert.Equal($"[\"{passengerId}\"]", savedTelemetry.Data["PassengerIds"]?.ToString());
            Assert.Equal(sliceId, savedTelemetry.Data["SliceId"]?.ToString());
            var affectedSlice = result.Passengers.Single().SliceRequirements!.Single();
            Assert.Equal(sliceId, affectedSlice.SliceId);
            var form = Assert.Single(affectedSlice.GovernmentForms!);
            Assert.Equal(GovernmentFormsStatus.COMPLETED, form.Status);
            Assert.Equal("32738f95-5023-4a23-bf8c-16dd388cbaff", form.Id);
            Assert.Equal("Maldives Traveller Declaration", form.Title);
            Assert.Equal("https://imuga.immigration.gov.mv/", form.Url);

            var payload = await GetRequestPayload();
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
            Assert.Contains($"\"sliceId\":\"{sliceId}\"", payload);
            Assert.Contains("\"id\":\"32738f95-5023-4a23-bf8c-16dd388cbaff\"", payload);
            Assert.Contains("\"linkId\":\"Maldives Traveller Declaration\"", payload);
            Assert.Contains("\"text\":\"Maldives Traveller Declaration\"", payload);
            Assert.Contains("\"href\":\"https://imuga.immigration.gov.mv/\"", payload);
            Assert.DoesNotContain("upload", payload, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("clicked", payload, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Mutation_ShouldUseStandardTravelRequirementsError_WhenSelectedFormIsNotCompleted()
        {
            const string passengerId = "01.01|required-government-forms-resource-id";
            const string sliceId = "slice-government-forms";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveGovernmentFormTravelRequirements(
                travelRequirementsId,
                passengerId,
                sliceId,
                "32738f95-5023-4a23-bf8c-16dd388cbaff",
                "Maldives Traveller Declaration",
                "Maldives Traveller Declaration",
                "https://imuga.immigration.gov.mv/",
                context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal(TravelRequirementsErrorMessages.DefaultFailureMessage, error.Message);
            Assert.Equal(TravelRequirementsErrorMessages.DefaultFailureTitle, error.Extensions["title"]);
            Assert.Equal(2, _telemetryService.Events.Count);
            var failedTelemetry = _telemetryService.Events[1];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED, failedTelemetry.Type);
            Assert.Equal("CNMLQN", failedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("FAILED", failedTelemetry.Data["Status"]?.ToString());
            Assert.Equal(LoggingFailureContext.SAVE_GOVERNMENT_FORM.ToString(), failedTelemetry.Data["TravelRequirementsStage"]?.ToString());
        }

        [Fact]
        public async Task Mutation_ShouldNotCallDependency_WhenSliceIsMissing()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveGovernmentFormTravelRequirements(
                "token",
                "passenger",
                null,
                "form",
                "link",
                "title",
                "https://government.example/form",
                context);

            Assert.Null(result);
            Assert.Equal("Missing mandatory field(s): sliceId", Assert.Single(resolverContext.Errors).Message);
            Assert.Empty(GetRequests());
        }

        [Fact]
        public async Task Mutation_ShouldUseStandardTravelRequirementsError_WhenDependencyFails()
        {
            const string passengerId = "01.01|unmatched-government-forms-resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.SaveGovernmentFormTravelRequirements(
                travelRequirementsId,
                passengerId,
                "unmatched-slice",
                "form",
                "link",
                "title",
                "https://government.example/form",
                context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal(TravelRequirementsErrorMessages.DefaultFailureMessage, error.Message);
            Assert.Equal(TravelRequirementsErrorMessages.DefaultFailureTitle, error.Extensions["title"]);
        }

        private async Task<string> GetRequestPayload()
        {
            var request = Assert.Single(GetRequests());
            return await request.Content!.ReadAsStringAsync(CancellationToken.None);
        }

        private IEnumerable<HttpRequestMessage> GetRequests()
        {
            var uri = $"{ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveGovernmentFormsAttestation";
            return _httpClientFactoryFixture.Current
                .GetResponses(uri)
                .Select(response => response.RequestMessage)
                .Where(request => request != null)!;
        }
    }
}
