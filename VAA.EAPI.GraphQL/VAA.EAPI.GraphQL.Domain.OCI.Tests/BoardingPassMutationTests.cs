using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using CheckInBoardingPass = VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass.BoardingPass;
using CheckInBoardingPassResponse = VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass.BoardingPassResponse;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class BoardingPassMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider; 

        public BoardingPassMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            Setup.CreateTestEnvironmentSettings();
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
                .AddSingleton<IAnalyticsService, AnalyticsService>()
                .AddSingleton<ILogAnalytics, LogAnalytics>()
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
                .AddSingleton<TestTelemetryService>()
                .AddSingleton<ITelemetryService>(sp => sp.GetRequiredService<TestTelemetryService>())
                .AddSingleton<BoardingPassMutation>()
                .BuildServiceProvider();
        }

        private (ICheckInServiceProvider checkInServiceProvider, ILogAnalytics logAnalytics, ITelemetryService telemetryService) GetConstructorDependencies()
        {
            return (
                _serviceProvider.GetRequiredService<ICheckInServiceProvider>(),
                _serviceProvider.GetRequiredService<ILogAnalytics>(),
                _serviceProvider.GetRequiredService<ITelemetryService>());
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCheckInServiceProviderIsNull()
        {
            // Arrange
            var (_, logAnalytics, telemetryService) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new BoardingPassMutation(null, logAnalytics, telemetryService));

            Assert.Equal("checkInMutationProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLogAnalyticsIsNull()
        {
            // Arrange
            var (checkInServiceProvider, _, telemetryService) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new BoardingPassMutation(checkInServiceProvider, null, telemetryService));

            Assert.Equal("logAnalytics", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenTelemetryIsNull()
        {
            // Arrange
            var (checkInServiceProvider, logAnalytics, _) = GetConstructorDependencies();

            // Act // Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new BoardingPassMutation(checkInServiceProvider, logAnalytics, null));

            Assert.Equal("telemetryService", exception.ParamName);
        }

        [Fact]
        public async Task Mutation_ShouldReturnError_NullCheckinId()
        {
            var _mutation = _serviceProvider.GetRequiredService<BoardingPassMutation>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var result = await _mutation.GenerateBoardingPass(null, context);

            // Assert
            Assert.Null(result);
            Assert.NotEmpty(resolverContext.Errors);
            Assert.IsType<HotChocolate.IError>(resolverContext.Errors[0], exactMatch: false);
            Assert.StartsWith("Mandatory fields missing in the request: checkInId", resolverContext.Errors[0].Message);
        }

        [Fact]
        public async Task Mutation_ShouldReturnNullBoardingPasses_ForAnyTechnicalError()
        {
            var _mutation = _serviceProvider.GetRequiredService<BoardingPassMutation>();
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "FakeTravelPartyId01", true, "FAKPNR", "JFK", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "FakeFirstName", "FakeLastName");

            var response = await _mutation.GenerateBoardingPass(checkInId, context);

            Assert.Null(response);
            Assert.NotEmpty(resolverContext.Errors);
            Assert.IsType<HotChocolate.IError>(resolverContext.Errors[0], exactMatch: false);
            Assert.Contains(resolverContext.Errors, e => e.Message.StartsWith("Looks like something needs sorting at the airport."));
            var refusedEvent = Assert.Single(telemetryService.Events.Where(e => (LoggingCheckinEvent)e.Type == LoggingCheckinEvent.CHECKIN_END_REFUSED));
            Assert.Equal("FAKPNR", refusedEvent.Data["PnrLocator"]?.ToString());
            Assert.Equal("JFK", refusedEvent.Data["OriginCode"]?.ToString());
        }       

        [Fact]
        public async Task Mutation_ShouldReturnBoardingPasses_OnSuccess()
        {
            var _mutation = _serviceProvider.GetRequiredService<BoardingPassMutation>();
            var telemetryService = _serviceProvider.GetRequiredService<TestTelemetryService>();
            var resolverContext = ResolverContextProxy.Create(out var context);

            var checkInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true, "travelparty-for-DQL7ST", true, "DQL7ST", "LHR", ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"], "FakeFirstName", "FakeLastName");

            var response = await _mutation.GenerateBoardingPass(checkInId, context);

            var boardingPasses = response.BoardingPasses.ToList();
            Assert.Equal(2, boardingPasses.Count);
            Assert.Equal(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/xcAAwMCAOq8yGQAAAAASUVORK5CYII=",
                boardingPasses.Single(boardingPass => boardingPass.OperatingFlightNumber == "VS185").DesktopBoardingPassBase64);
            Assert.Equal(
                "iVBORw0KGgoAAAANSUhEUgAAAAIAAAABCAQAAAB0L8Q7AAAAC0lEQVR42mP8z8AAAgMBAQ5X0bQAAAAASUVORK5CYII=",
                boardingPasses.Single(boardingPass => boardingPass.OperatingFlightNumber == "BW232").DesktopBoardingPassBase64);

            var successEvent = Assert.Single(telemetryService.Events.Where(e => (LoggingCheckinEvent)e.Type == LoggingCheckinEvent.CHECKIN_END_SUCCESS));
            Assert.Equal("DQL7ST", successEvent.Data["PnrLocator"]?.ToString());
            Assert.Equal("travelparty-for-DQL7ST", successEvent.Data["TravelPartyId"]?.ToString());
            var telemetryBoardingPasses = JArray.Parse(Assert.IsType<JValue>(successEvent.Data["BoardingPasses"]).Value<string>()!);

            Assert.All(telemetryBoardingPasses, boardingPass => Assert.True(boardingPass!["HasDesktopBoardingPass"]!.Value<bool>()));
            Assert.Contains(telemetryBoardingPasses, boardingPass => boardingPass!["HasQr"]!.Value<bool>());
            Assert.Contains(telemetryBoardingPasses, boardingPass => !boardingPass!["HasQr"]!.Value<bool>());
        }

        [Fact]
        public async Task DayOfTravel_ShouldLogQrAndDesktopBoardingPassPresence()
        {
            var response = new CheckInBoardingPassResponse
            {
                BoardingPasses =
                [
                    new CheckInBoardingPass
                    {
                        QrCode = "qr-code",
                        DesktopBoardingPassBase64 = "desktop-boarding-pass"
                    },
                    new CheckInBoardingPass()
                ]
            };

            var telemetry = await TelemetryHelper.DayOfTravel("DQL7ST", "LHR", "Virgin", "Test", response, null);
            var boardingPasses = JArray.Parse(Assert.IsType<string>(telemetry["BoardingPasses"]));

            Assert.Collection(
                boardingPasses,
                boardingPass =>
                {
                    Assert.True(boardingPass!["HasQr"]!.Value<bool>());
                    Assert.True(boardingPass["HasDesktopBoardingPass"]!.Value<bool>());
                },
                boardingPass =>
                {
                    Assert.False(boardingPass!["HasQr"]!.Value<bool>());
                    Assert.False(boardingPass["HasDesktopBoardingPass"]!.Value<bool>());
                });
        }
    }
}
