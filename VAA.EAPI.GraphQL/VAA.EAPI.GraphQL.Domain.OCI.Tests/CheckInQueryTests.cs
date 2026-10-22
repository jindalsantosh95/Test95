using System.IdentityModel.Tokens.Jwt;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class CheckInQueryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ICheckInServiceProvider _checkInServiceProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<AnalyticsService> _loggerAnalytics;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogAnalytics _logAnalytics;
        private readonly CheckInQuery _checkInQuery;
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<TelemetryService> _loggerTelemetry;
        private readonly ITelemetryService _telemetryService;
        private readonly TestTelemetryService _providerTelemetryService;

        public CheckInQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerAnalytics = Microsoft.Extensions.Logging.Abstractions.NullLogger<AnalyticsService>.Instance;
            _loggerTelemetry = Microsoft.Extensions.Logging.Abstractions.NullLogger<TelemetryService>.Instance;
            _telemetry = new TelemetryClient(new TelemetryConfiguration());
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _providerTelemetryService = new TestTelemetryService();
            _checkInServiceProvider = new CheckInServiceProvider(
                _httpConnector,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<CheckInServiceProvider>.Instance,
                telemetryService: _providerTelemetryService);
            _analyticsService = new AnalyticsService(_loggerAnalytics, _httpConnector);
            _logAnalytics = new LogAnalytics(_analyticsService);
            _telemetryService = new TelemetryService(_telemetry, _loggerTelemetry);
            _checkInQuery = new CheckInQuery(_checkInServiceProvider, _logAnalytics, _telemetryService);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenDependencyIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CheckInQuery(null, _logAnalytics, _telemetryService));
            Assert.Throws<ArgumentNullException>(() => new CheckInQuery(new Mock<ICheckInServiceProvider>().Object, null, _telemetryService));
            Assert.Throws<ArgumentNullException>(() => new CheckInQuery(new Mock<ICheckInServiceProvider>().Object, _logAnalytics, null));
        }

        [Fact]
        public async Task Query_ShouldReturnCheckInResponse_ForGoodResponse()
        {
            var result = await _checkInQuery.InitiateCheckIn("TEST02", null, "LHR", "LAMONT", "PAIVA", null);

            Assert.NotNull(result.OnlineCheckInStatus);
            Assert.IsType<Models.OnlineCheckInStatus>(result.OnlineCheckInStatus);
            Assert.True(result.OnlineCheckInStatus.Available);
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02-Id-SSCI", await CheckInIdHelper.GetTravelPartyId(result.CheckInId));
            Assert.Equal("TEST02-Id-TRM", await CheckInIdHelper.GetRequirementsId(result.CheckInId));
            Assert.Equal("TEST02", await CheckInIdHelper.GetPnrLocator(result.CheckInId));
            Assert.Equal(9, result.Passengers.Count());
            Assert.Equal("LAMONT", result.Passengers.First().GivenNames);
            Assert.Equal("PAIVA", result.Passengers.First().Surname);
            Assert.NotNull(result.Passengers.First().PassengerId);
            Assert.IsType<NodaTime.LocalDate>(result.Passengers.First().DateOfBirth);
            Assert.Equal(new LocalDate(1994, 10, 22), result.Passengers.First().DateOfBirth);
            Assert.Equal("447461649202", result.Passengers.First().Phone.Number);
            Assert.True(result.ApplyEmergencyContactToAllPax);
            Assert.False(result.ApplyContactTracingToAllPax);
            Assert.NotNull(result.CurrentSlice);
            Assert.IsType<Models.Slice>(result.CurrentSlice);
            Assert.IsType<Models.AirportAndTerminal>(result.CurrentSlice.Origin);
            Assert.IsType<Models.AirportAndTerminal>(result.CurrentSlice.Destination);
            Assert.Equal("Heathrow Airport", result.CurrentSlice.Origin.AirportName);
            Assert.Equal("GBR", result.CurrentSlice.Origin.CountryCode);
            Assert.Equal("John F Kennedy International", result.CurrentSlice.Destination.AirportName);
            Assert.DoesNotContain(_providerTelemetryService.Events, telemetryEvent => telemetryEvent.Type.Equals(LoggingCheckinEvent.CHECKIN_END_REFUSED));
        }

        [Fact]
        public async Task Query_ShouldReturnFailedCheckInResponseAndTrackRefusal_ForHttp404()
        {
            var result = await _checkInQuery.InitiateCheckIn("CHECKIN404", null, "LHR", "Test", "Passenger", null);

            Assert.NotNull(result);
            Assert.NotNull(result.OnlineCheckInStatus);
            Assert.False(result.OnlineCheckInStatus.Available);
            Assert.Equal("HTTP_404", result.OnlineCheckInStatus.Code);
            Assert.Contains(
                _providerTelemetryService.Events,
                telemetryEvent => telemetryEvent.Type.Equals(LoggingCheckinEvent.CHECKIN_END_REFUSED));
        }

        [Fact]
        public async Task CheckinEndRefused_ShouldIncludeInternalAdvisoryTextInTelemetry_WithoutExposingItInGraphQlResponse()
        {
            var graphQlResponse = await _checkInQuery.InitiateCheckIn("TSINIT", null, "LHR", "Test", "Passenger", null);
            var telemetryEvent = Assert.Single(_providerTelemetryService.Events.Where(telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingCheckinEvent.CHECKIN_END_REFUSED)));

            Assert.Equal("We're sorry. We can't validate your ticket.", telemetryEvent.Data["AdvisoryText1"]);
            Assert.DoesNotContain(typeof(Models.CheckInResponse).GetProperties(), property => property.Name == "AdvisoryText1");
            Assert.DoesNotContain(typeof(Models.OnlineCheckInStatus).GetProperties(), property => property.Name == "AdvisoryText1");
            Assert.False(graphQlResponse.OnlineCheckInStatus.Available);
        }

        [Fact]
        public async Task CheckinEndRefused_ShouldIncludeUnaccompaniedMinorAdvisoryTextInTelemetry_WithoutExposingItInGraphQlResponse()
        {
            var graphQlResponse = await _checkInQuery.InitiateCheckIn("UMNRPR", null, "LHR", "Minor", "Passenger", null);
            var telemetryEvent = Assert.Single(_providerTelemetryService.Events.Where(telemetryEvent =>
                telemetryEvent.Type.Equals(LoggingCheckinEvent.CHECKIN_END_REFUSED)));

            Assert.Equal("We can't check you in online if you wish to enroll in the Delta Unaccompanied Minor program. This requires you to check in at the airport with an agent.", telemetryEvent.Data["AdvisoryText1"]);
            Assert.DoesNotContain(typeof(Models.OnlineCheckInStatus).GetProperties(), property => property.Name == "AdvisoryText1");
            Assert.False(graphQlResponse.OnlineCheckInStatus.Available);
            Assert.Equal("UMNR", graphQlResponse.OnlineCheckInStatus.Code);
        }

        [Fact]
        public async Task ExtendSessions_ShouldThrowHttpRequestException_WhenSessionRefreshReturnsHttp404()
        {
            var expiredCheckInId = await CheckInIdHelper.CreateCheckInId(
                null,
                "requirements-id",
                false,
                "travel-party-id",
                false,
                "CHECKIN404",
                "LHR",
                null,
                "Test",
                "Passenger");

            await Assert.ThrowsAsync<HttpRequestException>(() => CheckInIdHelper.ExtendSessions(_checkInServiceProvider, expiredCheckInId));

            Assert.DoesNotContain(
                _providerTelemetryService.Events,
                telemetryEvent => telemetryEvent.Type.Equals(LoggingCheckinEvent.CHECKIN_END_REFUSED));
        }

        [Fact]
        public async Task ExtendSessions_ShouldThrowHttpRequestException_WhenPassengerReselectionReturnsHttp404()
        {
            var expiredCheckInId = await CheckInIdHelper.CreateCheckInId(
                null,
                "requirements-id",
                false,
                "travel-party-id",
                false,
                "CHECKINSELECT404",
                "LHR",
                ["passenger-1"],
                "Test",
                "Passenger");

            await Assert.ThrowsAsync<HttpRequestException>(() => CheckInIdHelper.ExtendSessions(_checkInServiceProvider, expiredCheckInId));

            Assert.DoesNotContain(
                _providerTelemetryService.Events,
                telemetryEvent => telemetryEvent.Type.Equals(LoggingCheckinEvent.CHECKIN_END_REFUSED));
        }
    }
}
