using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public sealed class SecureFlightInfoTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public SecureFlightInfoTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ITripsDetailsQueryProvider, TripsDetailsQueryProvider>()
                .AddSingleton<TestTelemetryService>()
                .AddSingleton<ITelemetryService>(source => source.GetRequiredService<TestTelemetryService>())
                .AddLogging()
                .AddSingleton<SecureFlightInfoQuery>()
                .AddSingleton<SecureFlightInfoMutation>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsNullAndReportsError_WhenMandatoryRequestFieldIsMissing()
        {
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await query.GetSecureFlightInfo(new SecureFlightInfoRequest
            {
                PnrLocator = string.Empty,
                GivenNames = "Test",
                Surname = "Passenger",
                PassengerFirstNin = "01",
                PassengerLastNin = "01"
            }, context);

            Assert.Null(result);
            Assert.Equal("Missing mandatory field(s): PnrLocator", Assert.Single(resolverContext.Errors).Message);
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsNullAndReportsError_WhenRequestIsNull()
        {
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await query.GetSecureFlightInfo(null, context);

            Assert.Null(result);
            Assert.Equal("Missing mandatory field(s): request", Assert.Single(resolverContext.Errors).Message);
        }

        [Fact]
        public async Task GetSecureFlightInfo_TracksInitiationTelemetry_WhenRequestIsValid()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            await query.GetSecureFlightInfo(CreateValidQueryRequest(), context);

            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_REQUESTED));
            Assert.Equal("ERRG6O", telemetryEvent.Data["PnrLocator"]?.ToString());
            Assert.Equal("01", telemetryEvent.Data["PassengerFirstNin"]?.ToString());
            Assert.Equal("02", telemetryEvent.Data["PassengerLastNin"]?.ToString());
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsPassengerDetailsAndTracksRetrievedTelemetry_WhenRequestIsValid()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await query.GetSecureFlightInfo(CreateValidQueryRequest(), context);

            Assert.NotNull(result);
            Assert.Equal("Test-booking-Id", result.SecureFlightReservationId);
            Assert.Equal("5e9e1255-b51a-49a1-9cfa-ecd40b46a4b0", result.PassengerDetails.PassengerId);
            Assert.Equal("987654321", result.PassengerDetails.KnownTravellerNumber);
            Assert.Equal("CAN1234567890", result.PassengerDetails.CanadianTravelNumber);
            Assert.Null(result.PassengerDetails.RedressNumber);
            Assert.True(result.ShowRedressNumber);
            Assert.True(result.ShowCanadianTravelNumber);
            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_RETRIEVED));
            Assert.Equal("ERRG6O", telemetryEvent.Data["PnrLocator"]?.ToString());
            Assert.Equal("Test-booking-Id", telemetryEvent.Data["SecureFlightReservationId"]?.ToString());
            Assert.Equal("5e9e1255-b51a-49a1-9cfa-ecd40b46a4b0", telemetryEvent.Data["PassengerId"]?.ToString());
            Assert.Equal("01", telemetryEvent.Data["PassengerFirstNin"]?.ToString());
            Assert.Equal("02", telemetryEvent.Data["PassengerLastNin"]?.ToString());
            Assert.Equal("True", telemetryEvent.Data["HasKnownTravellerNumber"]?.ToString());
            Assert.Equal("True", telemetryEvent.Data["HasCanadianTravelNumber"]?.ToString());
            Assert.Equal("False", telemetryEvent.Data["HasRedressNumber"]?.ToString());
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsDestinationFlags_WhenOnlyCanadaIsOnTheItinerary()
        {
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);
            var request = CreateValidQueryRequest();
            request.PnrLocator = "CAONLY";

            var result = await query.GetSecureFlightInfo(request, context);

            Assert.NotNull(result);
            Assert.False(result.ShowRedressNumber);
            Assert.True(result.ShowCanadianTravelNumber);
            Assert.Empty(resolverContext.Errors);
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsDefaultError_WhenTripsSearchReturnsBadRequest()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);
            var request = CreateValidQueryRequest();
            request.PnrLocator = "ERROR400";

            var result = await query.GetSecureFlightInfo(request, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal(SecureFlightInfoErrorMessages.DefaultFailureMessage, error.Message);
            Assert.Equal(SecureFlightInfoErrorMessages.DefaultFailureTitle, error.Extensions["title"]?.ToString());
            Assert.Equal("ROR4082", error.Extensions["code"]?.ToString());
            AssertFailedTelemetryEvent(telemetryService, "ERROR400", "ROR4082");
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsDefaultError_WhenPassengerNinIsNotInPassengerDetails()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);
            var request = new SecureFlightInfoRequest
            {
                PnrLocator = "ERRG6O",
                GivenNames = "ONEPSG",
                Surname = "TEST",
                PassengerFirstNin = "01",
                PassengerLastNin = "04"
            };

            var result = await query.GetSecureFlightInfo(request, context);

            Assert.Null(result);
            var error = Assert.Single(resolverContext.Errors);
            Assert.Equal(SecureFlightInfoErrorMessages.DefaultFailureMessage, error.Message);
            Assert.Equal("500", error.Extensions["code"]?.ToString());
            AssertFailedTelemetryEvent(telemetryService, "ERRG6O");
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsDefaultError_WhenTripBookingIsNotFound()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);
            var request = CreateValidQueryRequest();
            request.PnrLocator = "NOBOOKING";

            var result = await query.GetSecureFlightInfo(request, context);

            Assert.Null(result);
            Assert.Equal(SecureFlightInfoErrorMessages.DefaultFailureMessage, Assert.Single(resolverContext.Errors).Message);
            AssertFailedTelemetryEvent(telemetryService, "NOBOOKING");
        }

        [Fact]
        public async Task GetSecureFlightInfo_ReturnsDefaultError_WhenPassengerDetailsAreNotAvailable()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var query = _serviceProvider.GetRequiredService<SecureFlightInfoQuery>();
            var resolverContext = ResolverContextProxy.Create(out var context);
            var request = CreateValidQueryRequest();
            request.PnrLocator = "NOPASSENGERDETAILS";

            var result = await query.GetSecureFlightInfo(request, context);

            Assert.Null(result);
            Assert.Equal(SecureFlightInfoErrorMessages.DefaultFailureMessage, Assert.Single(resolverContext.Errors).Message);
            AssertFailedTelemetryEvent(telemetryService, "NOPASSENGERDETAILS");
        }

        [Fact]
        public async Task UpdateSecureFlightInfo_ReturnsNullAndReportsError_WhenNoSecureFlightDetailNumberIsProvided()
        {
            var mutation = _serviceProvider.GetRequiredService<SecureFlightInfoMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.UpdateSecureFlightInfo(new UpdateSecureFlightInfoRequest
            {
                SecureFlightReservationId = "booking-id",
                PassengerId = "passenger-id",
                SecureFlightDetails = []
            }, context);

            Assert.Null(result);
            Assert.Equal("At least one secure flight detail must be provided.", Assert.Single(resolverContext.Errors).Message);
        }

        [Fact]
        public async Task UpdateSecureFlightInfo_ReturnsNullAndReportsError_WhenRequestIsNull()
        {
            var mutation = _serviceProvider.GetRequiredService<SecureFlightInfoMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.UpdateSecureFlightInfo(null, context);

            Assert.Null(result);
            Assert.Equal("Missing mandatory field(s): request", Assert.Single(resolverContext.Errors).Message);
        }

        [Fact]
        public async Task UpdateSecureFlightInfo_ReturnsNullAndReportsError_WhenMandatoryRequestFieldIsMissing()
        {
            var mutation = _serviceProvider.GetRequiredService<SecureFlightInfoMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var request = CreateValidMutationRequest();
            request.PassengerId = string.Empty;

            var result = await mutation.UpdateSecureFlightInfo(request, context);

            Assert.Null(result);
            Assert.Equal("Missing mandatory field(s): PassengerId", Assert.Single(resolverContext.Errors).Message);
        }

        [Fact]
        public async Task UpdateSecureFlightInfo_ReturnsNullAndReportsError_WhenCanadianTravelAndRedressNumbersAreBothProvided()
        {
            var mutation = _serviceProvider.GetRequiredService<SecureFlightInfoMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.UpdateSecureFlightInfo(new UpdateSecureFlightInfoRequest
            {
                SecureFlightReservationId = "booking-id",
                PassengerId = "passenger-id",
                SecureFlightDetails =
                [
                    new SecureFlightDetailRequest { Type = SecureFlightDetailType.CanadianTravelNumber, Number = "CAN1234567890" },
                    new SecureFlightDetailRequest { Type = SecureFlightDetailType.RedressNumber, Number = "456789012" }
                ]
            }, context);

            Assert.Null(result);
            Assert.Equal("Canadian Travel Number and Redress Number cannot be provided together.", Assert.Single(resolverContext.Errors).Message);
        }

        [Fact]
        public async Task UpdateSecureFlightInfo_TracksInitiationTelemetry_WhenRequestIsValid()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var mutation = _serviceProvider.GetRequiredService<SecureFlightInfoMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            await mutation.UpdateSecureFlightInfo(CreateValidMutationRequest(), context);

            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_UPDATE_INITIATED));
            Assert.Equal("booking-id", telemetryEvent.Data["SecureFlightReservationId"]?.ToString());
            Assert.Equal("01", telemetryEvent.Data["PassengerFirstNin"]?.ToString());
            Assert.Equal("02", telemetryEvent.Data["PassengerLastNin"]?.ToString());
        }

        [Fact]
        public async Task UpdateSecureFlightInfo_ReturnsPassengerDetailsAndTracksUpdatedTelemetry_WhenRequestIsValid()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var mutation = _serviceProvider.GetRequiredService<SecureFlightInfoMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await mutation.UpdateSecureFlightInfo(new UpdateSecureFlightInfoRequest
            {
                SecureFlightReservationId = "Test-booking-Id",
                PassengerId = "5e9e1255-b51a-49a1-9cfa-ecd40b46a4b0",
                SecureFlightDetails =
                [
                    new SecureFlightDetailRequest { Type = SecureFlightDetailType.KnownTravellerNumber, Number = "987654321" },
                    new SecureFlightDetailRequest { Type = SecureFlightDetailType.CanadianTravelNumber, Number = "CAN1234567890" }
                ]
            }, context);

            Assert.NotNull(result);
            Assert.Equal("Test-booking-Id", result.SecureFlightReservationId);
            Assert.Equal("5e9e1255-b51a-49a1-9cfa-ecd40b46a4b0", result.PassengerDetails.PassengerId);
            Assert.Equal("987654321", result.PassengerDetails.KnownTravellerNumber);
            Assert.Equal("CAN1234567890", result.PassengerDetails.CanadianTravelNumber);
            Assert.Null(result.ShowRedressNumber);
            Assert.Null(result.ShowCanadianTravelNumber);
            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_UPDATED));
            Assert.Equal("Test-booking-Id", telemetryEvent.Data["SecureFlightReservationId"]?.ToString());
            Assert.Equal("5e9e1255-b51a-49a1-9cfa-ecd40b46a4b0", telemetryEvent.Data["PassengerId"]?.ToString());
            Assert.Equal("01", telemetryEvent.Data["PassengerFirstNin"]?.ToString());
            Assert.Equal("02", telemetryEvent.Data["PassengerLastNin"]?.ToString());
            Assert.Equal("True", telemetryEvent.Data["HasKnownTravellerNumber"]?.ToString());
            Assert.Equal("True", telemetryEvent.Data["HasCanadianTravelNumber"]?.ToString());
            Assert.Equal("False", telemetryEvent.Data["HasRedressNumber"]?.ToString());
        }

        [Fact]
        public async Task UpdateSecureFlightInfo_TracksFailureTelemetry_WhenTripMsPatchFails()
        {
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var mutation = _serviceProvider.GetRequiredService<SecureFlightInfoMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);
            var request = CreateValidMutationRequest();
            request.SecureFlightReservationId = "PATCHFAILURE";

            var result = await mutation.UpdateSecureFlightInfo(request, context);

            Assert.Null(result);
            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_FAILED));
            Assert.Equal("FAILED", telemetryEvent.Data["Status"]?.ToString());
            Assert.Equal(LoggingFailureContext.UPDATE_SECURE_FLIGHT_INFO.ToString(), telemetryEvent.Data["SecureFlightInfoStage"]?.ToString());
            var telemetryError = telemetryEvent.Data["SecureFlightInfoError"]?.ToString() ?? string.Empty;
            Assert.Contains("PATCH400", telemetryError);
            Assert.DoesNotContain("Internal Delta diagnostic", telemetryError);
            Assert.DoesNotContain("Delta", telemetryError);
        }

        private static SecureFlightInfoRequest CreateValidQueryRequest() => new()
        {
            PnrLocator = "ERRG6O",
            GivenNames = "ONEPSG",
            Surname = "TEST",
            PassengerFirstNin = "01",
            PassengerLastNin = "02"
        };

        private static UpdateSecureFlightInfoRequest CreateValidMutationRequest() => new()
        {
            SecureFlightReservationId = "booking-id",
            PassengerId = "passenger-id",
            PassengerFirstNin = "01",
            PassengerLastNin = "02",
            SecureFlightDetails =
            [
                new SecureFlightDetailRequest { Type = SecureFlightDetailType.KnownTravellerNumber, Number = "123456789" }
            ]
        };

        private static void AssertFailedTelemetryEvent(TestTelemetryService telemetryService, string pnrLocator, string? errorCode = null)
        {
            var telemetryEvent = Assert.Single(telemetryService.Events, telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingTravelRequirementsEvent.TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_FAILED));

            Assert.Equal(pnrLocator, telemetryEvent.Data["PnrLocator"]?.ToString());
            Assert.Equal("FAILED", telemetryEvent.Data["Status"]?.ToString());
            Assert.Equal(LoggingFailureContext.GET_SECURE_FLIGHT_INFO.ToString(), telemetryEvent.Data["SecureFlightInfoStage"]?.ToString());

            if (errorCode is not null)
                Assert.Contains(errorCode, telemetryEvent.Data["SecureFlightInfoError"]?.ToString() ?? string.Empty);
        }

    }
}
