using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class SaveReturnDateMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ICheckInServiceProvider _checkInServiceProvider;
        private IReferenceServiceProvider _referenceServiceProvider;
        private readonly SaveReturnDateMutation _mutation;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<AnalyticsService> _loggerAnalytics;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogAnalytics _logAnalytics;
        private readonly CheckInQuery _checkInQuery;
        private readonly TelemetryClient _telemetry;
        private readonly ILogger<TelemetryService> _loggerTelemetry;
        private readonly ITelemetryService _telemetryService;
        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

        public SaveReturnDateMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerAnalytics = Microsoft.Extensions.Logging.Abstractions.NullLogger<AnalyticsService>.Instance;
            _loggerTelemetry = Microsoft.Extensions.Logging.Abstractions.NullLogger<TelemetryService>.Instance;
            _telemetry = new TelemetryClient(new TelemetryConfiguration());
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _checkInServiceProvider = new CheckInServiceProvider(_httpConnector);
            _referenceServiceProvider = new ReferenceServiceProvider(_httpConnector);
            _analyticsService = new AnalyticsService(_loggerAnalytics, _httpConnector);
            _logAnalytics = new LogAnalytics(_analyticsService);
            _telemetryService = new TelemetryService(_telemetry, _loggerTelemetry);
            _checkInQuery = new CheckInQuery(_checkInServiceProvider, _logAnalytics, _telemetryService);
            _mutation = new SaveReturnDateMutation(_checkInServiceProvider, _referenceServiceProvider, _logAnalytics, _telemetryService);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(null, _referenceServiceProvider, _logAnalytics, _telemetryService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenReferenceServiceProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(_checkInServiceProvider, null, _logAnalytics, _telemetryService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLogAnalyticsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(_checkInServiceProvider, _referenceServiceProvider, null, _telemetryService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SavePassportMutation(_checkInServiceProvider, _referenceServiceProvider, _logAnalytics, null));
        }

        [Fact]
        public async Task Mutation_ShouldThrowException_ForNullRequest()
        {
            //Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _mutation.SaveReturnDate(null, null));
        }

        [Fact]
        public async Task Mutation_ShouldReturnCheckiResponse_IfSecondaryDocReqidNotNeeded()
        {
            var checkInResponse = await _checkInQuery.InitiateCheckIn("TEST02", null, "LHR", "TORI", "PAIK", null);
            //Act
            var result = await _mutation.SaveReturnDate(new Models.SaveReturnDate.SaveReturnDateRequest
            {
                CheckInId = checkInResponse.CheckInId,
                PassengerId = "01.01|77456950-f219-4053-8008-5ee8bff65a32|9999225479295|14ed3a2a-a726-4754-af01-872b02134712",
                NoReturnDate = false,
                ReturnOn = LocalDateTime.FromDateTime(DateTime.UtcNow.AddMonths(1)).Date
            }, null);

            //Assert
            Assert.NotNull(result.OnlineCheckInStatus);
            Assert.IsType<OnlineCheckInStatus>(result.OnlineCheckInStatus);
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02", await CheckInIdHelper.GetPnrLocator(result.CheckInId));
            Assert.NotEmpty(result.Passengers.FirstOrDefault().Documents);
            Assert.True(result.Passengers.FirstOrDefault().Requirements.PassportProvided);
            Assert.False(result.Passengers.FirstOrDefault().Requirements.PassportVerified);
            Assert.Equal(ReturnDateStatus.PROVIDED, result.Passengers.FirstOrDefault().Requirements.ReturnDateStatus);
            Assert.NotNull(result.Passengers.FirstOrDefault().Requirements.ReturnDate);
            Assert.Equal(SecondaryDocumentStatus.NOT_REQUIRED, result.Passengers.FirstOrDefault().Requirements.SecondaryDocumentStatus);
            Assert.True(result.Passengers.FirstOrDefault().Requirements.CheckinEligible);
            Assert.False(result.Passengers.FirstOrDefault().Requirements.CheckedIn);
        }

        [Fact]
        public async Task Mutation_ShouldReturnCheckiResponse_IfSecondaryDocReqidNotNeeded_ReturnDate()
        {
            // This test is the same as the one above, but passes the new ReturnOn field rather than the old ReturnDate

            var checkInResponse = await _checkInQuery.InitiateCheckIn("TEST02", null, "LHR", "TORI", "PAIK", null);
            //Act
            var result = await _mutation.SaveReturnDate(new Models.SaveReturnDate.SaveReturnDateRequest
            {
                CheckInId = checkInResponse.CheckInId,
                PassengerId = "01.01|77456950-f219-4053-8008-5ee8bff65a32|9999225479295|14ed3a2a-a726-4754-af01-872b02134712",
                NoReturnDate = false,
                ReturnDate = DateTime.UtcNow.AddMonths(1)
            }, null);

            //Assert
            Assert.NotNull(result.OnlineCheckInStatus);
            Assert.IsType<OnlineCheckInStatus>(result.OnlineCheckInStatus);
            Assert.NotNull(result.CheckInId);
            Assert.True(jwtSecurityTokenHandler.CanReadToken(result.CheckInId));
            Assert.Equal("TEST02", await CheckInIdHelper.GetPnrLocator(result.CheckInId));
            Assert.NotEmpty(result.Passengers.FirstOrDefault().Documents);
            Assert.True(result.Passengers.FirstOrDefault().Requirements.PassportProvided);
            Assert.False(result.Passengers.FirstOrDefault().Requirements.PassportVerified);
            Assert.Equal(ReturnDateStatus.PROVIDED, result.Passengers.FirstOrDefault().Requirements.ReturnDateStatus);
            Assert.NotNull(result.Passengers.FirstOrDefault().Requirements.ReturnDate);
            Assert.Equal(SecondaryDocumentStatus.NOT_REQUIRED, result.Passengers.FirstOrDefault().Requirements.SecondaryDocumentStatus);
            Assert.True(result.Passengers.FirstOrDefault().Requirements.CheckinEligible);
            Assert.False(result.Passengers.FirstOrDefault().Requirements.CheckedIn); ;
        }
    }
}
