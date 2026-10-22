using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.TravelRequirements;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class FailureHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public FailureHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ITravelRequirementsServiceProvider, TravelRequirementsServiceProvider>()
                .AddSingleton<TestTelemetryService>()
                .AddSingleton<ITelemetryService>(source => source.GetRequiredService<TestTelemetryService>())
                .AddLogging()
                .AddSingleton<TravelRequirementsSaveReturnDateMutation>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task TravelRequirementsSaveReturnDateMutation_ShouldReturnParsedGraphQLError_ForHttpFailure()
        {
            var mutation = _serviceProvider.GetRequiredService<TravelRequirementsSaveReturnDateMutation>();
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var passengerId = "01.01|business-error-resource-id";
            var travelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, "requirements-id", true, "TRAVEL", "TEST", "CNMLQN", [passengerId]);
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.SaveReturnDate(new TravelRequirementsSaveReturnDateRequest
            {
                TravelRequirementsId = travelRequirementsId,
                PassengerId = passengerId,
                ReturnOn = new LocalDate(2026, 10, 1)
            }, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("We can’t load the travel documents step at the moment. Please try again shortly.", error.Message);
            Assert.Equal("travelrequirement_011_001", error.Extensions["code"]?.ToString());
            Assert.Contains(telemetryService.Events, telemetryEvent => telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED));
        }

        [Fact]
        public async Task TravelRequirementsFailureHandler_ShouldReturnDefaultError_WhenTravelRequirementsIdCannotBeDecoded()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);
            var telemetryService = new TestTelemetryService();

            var result = await TravelRequirementsFailureHandler.HandleAsync(
                context,
                new HttpRequestException("Travel requirements service unavailable.", null, HttpStatusCode.BadGateway),
                new TravelRequirementsFailureContext
                {
                    TravelRequirementsId = "invalid-travel-requirements-id",
                    LoggingFailureContext = LoggingFailureContext.SAVE_RETURN_DATE,
                    Operation = "save return date",
                    DefaultMessage = "Default message",
                    DefaultTitle = "Default title"
                },
                telemetryService,
                NullLogger.Instance);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Default message", error.Message);
            Assert.Equal("500", error.Extensions["code"]?.ToString());
        }

        [Fact]
        public async Task BaggageFailureHandler_ShouldReturnExistingGenericGraphQLError_ForHttpFailure()
        {
            var checkInId = await CheckInIdHelper.CreateCheckInId(
                null,
                "requirements-id",
                true,
                "travel-party-id",
                true,
                "TEST01",
                "LHR");
            var resolverContext = ResolverContextProxy.Create(out var context);
            var telemetryService = new TestTelemetryService();

            var result = await BaggageFailureHandler.HandleAsync<BaggageResponse>(
                context,
                new HttpRequestException("Baggage service unavailable.", null, HttpStatusCode.BadGateway),
                new BaggageFailureContext
                {
                    CheckInId = checkInId,
                    LoggingFailureContext = LoggingFailureContext.BAGGAGE_OFFERS_RETRIEVAL,
                    Operation = "retrieve baggage offers"
                },
                telemetryService,
                NullLogger.Instance);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal(Application.BaggageCheckInFailDefaultMessage, error.Message);
            Assert.Equal(Application.BaggageCheckInFailDefaultTiTle, error.Extensions["title"]?.ToString());
            Assert.Equal("500", error.Extensions["code"]?.ToString());
            Assert.Contains(telemetryService.Events, telemetryEvent => telemetryEvent.Type.Equals(LoggingCheckinEvent.BAGGAGE_DETAILS_FAILED));
        }

        [Fact]
        public async Task TravelRequirementsFailureHandler_ShouldReturnParsedGraphQLError_WhenConfiguredToReturnParsedError()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);
            var telemetryService = new TestTelemetryService();
            var exception = new HttpRequestException("Travel requirements service rejected the request.", null, HttpStatusCode.UnprocessableEntity);
            exception.Data["Response"] = "[{\"code\":\"travelrequirement_030_001\",\"message\":\"The surname and given name entered does not match the reservation.\",\"title\":\"Travel requirements error\"}]";

            var result = await TravelRequirementsFailureHandler.HandleAsync(
                context,
                exception,
                new TravelRequirementsFailureContext
                {
                    PnrLocator = "TEST01",
                    LoggingFailureContext = LoggingFailureContext.SAVE_CONTACT_TRACING,
                    Operation = "save contact tracing",
                    DefaultMessage = "Default message",
                    DefaultTitle = "Default title"
                },
                telemetryService,
                NullLogger.Instance);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("The surname and given name entered does not match the reservation.", error.Message);
            Assert.Equal("Travel requirements error", error.Extensions["title"]?.ToString());
            Assert.Equal("travelrequirement_030_001", error.Extensions["code"]?.ToString());
            Assert.Contains(telemetryService.Events, telemetryEvent => telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED));
        }

        [Fact]
        public async Task TravelRequirementsFailureHandler_ShouldReturnDomesticOnlyError_WhenRetrievalUsesGenericErrors()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);
            var telemetryService = new TestTelemetryService();
            var exception = new HttpRequestException("Travel requirements service rejected the request.", null, HttpStatusCode.UnprocessableEntity);
            exception.Data["Response"] = "[{\"code\":\"travelrequirement_020_003\",\"message\":\"Your domestic flight has no special travel requirements. However, government or health requirements may still apply. Please check at the airport.\",\"title\":\"\"}]";

            var result = await TravelRequirementsFailureHandler.HandleAsync(
                context,
                exception,
                new TravelRequirementsFailureContext
                {
                    PnrLocator = "TEST01",
                    LoggingFailureContext = LoggingFailureContext.GET_TRAVEL_REQUIREMENTS,
                    Operation = "retrieve travel requirements",
                    DefaultMessage = "Default message",
                    DefaultTitle = "Default title",
                    ReturnParsedError = false
                },
                telemetryService,
                NullLogger.Instance);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Your domestic flight has no special travel requirements. However, government or health requirements may still apply. Please check at the airport.", error.Message);
            Assert.Equal(string.Empty, error.Extensions["title"]?.ToString());
            Assert.Equal("travelrequirement_020_003", error.Extensions["code"]?.ToString());

            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent => telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED));
            Assert.Equal("TEST01", telemetryEvent.Data["PnrLocator"]?.ToString());
            Assert.Equal("FAILED", telemetryEvent.Data["Status"]?.ToString());
            Assert.Equal(LoggingFailureContext.GET_TRAVEL_REQUIREMENTS.ToString(), telemetryEvent.Data["TravelRequirementsStage"]?.ToString());

            var telemetryError = telemetryEvent.Data["PassportTRMError"]?.ToString();
            Assert.Contains("travelrequirement_020_003", telemetryError);
            Assert.Contains("Your domestic flight has no special travel requirements.", telemetryError);
        }

        [Fact]
        public async Task TravelRequirementsFailureHandler_ShouldRedactParsedErrorInTelemetry_WhenReturningGenericError()
        {
            var resolverContext = ResolverContextProxy.Create(out var context);
            var telemetryService = new TestTelemetryService();
            var exception = new HttpRequestException("Travel requirements service rejected the request.", null, HttpStatusCode.UnprocessableEntity);
            exception.Data["Response"] = "[{\"code\":\"travelrequirement_030_001\",\"message\":\"The surname and given name entered does not match the reservation.\",\"developerMessage\":\"Sensitive downstream diagnostic detail\",\"title\":\"Travel requirements error\"}]";

            var result = await TravelRequirementsFailureHandler.HandleAsync(
                context,
                exception,
                new TravelRequirementsFailureContext
                {
                    PnrLocator = "TEST01",
                    LoggingFailureContext = LoggingFailureContext.GET_TRAVEL_REQUIREMENTS,
                    Operation = "retrieve travel requirements",
                    DefaultMessage = "Default message",
                    DefaultTitle = "Default title",
                    ReturnParsedError = false
                },
                telemetryService,
                NullLogger.Instance);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal("Default message", error.Message);

            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent => telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SESSION_FAILED));
            var telemetryError = telemetryEvent.Data["PassportTRMError"]?.ToString();
            Assert.Contains("travelrequirement_030_001", telemetryError);
            Assert.DoesNotContain("Sensitive downstream diagnostic detail", telemetryError);
        }

    }
}
