using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public sealed class SaveSecondaryDocumentTravelRequirementsTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public SaveSecondaryDocumentTravelRequirementsTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ITravelRequirementsServiceProvider, TravelRequirementsServiceProvider>()
                .AddSingleton<TestTelemetryService>()
                .AddSingleton<ITelemetryService>(serviceProvider => serviceProvider.GetRequiredService<TestTelemetryService>())
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddSingleton<SaveSecondaryDocumentMutation>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Mutation_ShouldReturnCompletedRefreshedState_ForTargetPassengerAndSlice()
        {
            var mutation = _serviceProvider.GetRequiredService<SaveSecondaryDocumentMutation>();
            var jwtSecurityTokenHandler = _serviceProvider.GetRequiredService<JwtSecurityTokenHandler>();
            var resolverContext = ResolverContextProxy.Create(out var context);
            const string passengerId = "01.01|secondary-document-resource-id";
            const string sliceId = "slice-secondary-document";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);

            var result = await mutation.SaveSecondaryDocumentTravelRequirements(
                travelRequirementsId, passengerId, sliceId, "ETA_1", "US", context);

            Assert.Empty(resolverContext.Errors);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.TravelRequirementsId));
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            Assert.Equal(2, telemetryService.Events.Count);
            var submittedTelemetry = telemetryService.Events[0];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECONDARY_DOCUMENT_SUBMITTED, submittedTelemetry.Type);
            Assert.Equal("CNMLQN", submittedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("requirements-id", submittedTelemetry.Data["RequirementsId"]?.ToString());
            Assert.Equal(passengerId, submittedTelemetry.Data["PassengerId"]?.ToString());
            Assert.Equal(sliceId, submittedTelemetry.Data["SliceId"]?.ToString());

            var savedTelemetry = telemetryService.Events[1];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECONDARY_DOCUMENT_SAVED, savedTelemetry.Type);
            Assert.Equal("CNMLQN", savedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("requirements-id", savedTelemetry.Data["RequirementsId"]?.ToString());
            Assert.Equal($"[\"{passengerId}\"]", savedTelemetry.Data["PassengerIds"]?.ToString());
            Assert.Equal(sliceId, savedTelemetry.Data["SliceId"]?.ToString());
            var secondaryDocument = Assert.Single(result.Passengers.Single().SliceRequirements!.Single().SecondaryDocuments);
            Assert.Equal(SecondaryDocumentStatus.COMPLETE, secondaryDocument.Status);
            Assert.Equal("US", secondaryDocument.CountryCode);
            Assert.Equal("USA", secondaryDocument.CountryCode3Letter);
            Assert.Equal("ETA", secondaryDocument.Name);
            Assert.Equal("ESTA", secondaryDocument.DisplayName);
            Assert.Equal(2, secondaryDocument.SecondaryDocumentApplicationOptions.Count);
            Assert.All(secondaryDocument.SecondaryDocumentApplicationOptions, application => Assert.Equal("ETA_1", application.SecondaryDocumentId));
            var vendorApplication = Assert.Single(secondaryDocument.SecondaryDocumentApplicationOptions, application => application.ApplicationType == ApplicationType.Vendor);
            Assert.Equal("Apply with Sherpa", vendorApplication.Title);
            Assert.Equal("Apply with Sherpa", vendorApplication.UrlText);
            var governmentApplication = Assert.Single(secondaryDocument.SecondaryDocumentApplicationOptions, application => application.ApplicationType == ApplicationType.Government);
            Assert.Equal("https://esta.cbp.dhs.gov/", governmentApplication.Url);
            Assert.Equal("Apply on government website", governmentApplication.UrlText);

            var payload = await GetRequestPayload();
            Assert.Contains("\"requirementsId\":\"requirements-id\"", payload);
            Assert.Contains($"\"passengerId\":\"{passengerId}\"", payload);
            Assert.Contains("\"secondaryDocumentId\":\"ETA_1\"", payload);
            Assert.DoesNotContain("\"name\"", payload);
            Assert.DoesNotContain("\"text\"", payload);
            Assert.DoesNotContain("\"href\"", payload);
            Assert.DoesNotContain("\"applicationType\"", payload);
        }

        [Theory]
        [InlineData(null, "passenger", "slice", "ETA_1", "US", "travelRequirementsId")]
        [InlineData("token", null, "slice", "ETA_1", "US", "passengerId")]
        [InlineData("token", "passenger", null, "ETA_1", "US", "sliceId")]
        [InlineData("token", "passenger", "slice", null, "US", "secondaryDocumentId")]
        [InlineData("token", "passenger", "slice", "ETA_1", null, "countryCode")]
        public async Task Mutation_ShouldNotCallDependency_WhenMandatoryRequestAttributeIsMissing(
            string? travelRequirementsId,
            string? passengerId,
            string? sliceId,
            string? secondaryDocumentId,
            string? countryCode,
            string missingField)
        {
            var mutation = _serviceProvider.GetRequiredService<SaveSecondaryDocumentMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.SaveSecondaryDocumentTravelRequirements(
                travelRequirementsId, passengerId, sliceId, secondaryDocumentId, countryCode, context);

            Assert.Null(result);
            Assert.Equal($"Missing mandatory field(s): {missingField}", Assert.Single(resolverContext.Errors).Message);
            Assert.Empty(GetRequests());
        }

        [Fact]
        public async Task Mutation_ShouldTrackFailedTelemetry_WhenDependencyFails()
        {
            var mutation = _serviceProvider.GetRequiredService<SaveSecondaryDocumentMutation>();
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            const string passengerId = "01.01|unmatched-secondary-document-resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(
                null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.SaveSecondaryDocumentTravelRequirements(
                travelRequirementsId, passengerId, "unmatched-slice", "ETA_1", "US", context);

            Assert.Null(result);
            Assert.Single(resolverContext.Errors);
            Assert.Equal(2, telemetryService.Events.Count);
            var failedTelemetry = telemetryService.Events[1];
            Assert.Equal(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED, failedTelemetry.Type);
            Assert.Equal("CNMLQN", failedTelemetry.Data["PnrLocator"]?.ToString());
            Assert.Equal("FAILED", failedTelemetry.Data["Status"]?.ToString());
            Assert.Equal(LoggingFailureContext.SAVE_SECONDARY_DOCUMENT.ToString(), failedTelemetry.Data["TravelRequirementsStage"]?.ToString());
        }

        private async Task<string> GetRequestPayload()
        {
            var request = Assert.Single(GetRequests());
            return await request.Content!.ReadAsStringAsync(CancellationToken.None);
        }

        private IEnumerable<HttpRequestMessage> GetRequests()
        {
            var uri = $"{ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.CheckInMSUrl)}/TravelRequirements/SaveSecondaryDocumentAttestation";
            var httpClientFactory = (Core.Xunit.Http.HttpClientFactory)_serviceProvider.GetRequiredService<IHttpClientFactory>();

            return httpClientFactory.GetResponses(uri)
                .Select(response => response.RequestMessage)
                .Where(request => request?.RequestUri?.AbsolutePath.EndsWith(
                    "/TravelRequirements/SaveSecondaryDocumentAttestation",
                    StringComparison.OrdinalIgnoreCase) == true)!;
        }
    }
}
