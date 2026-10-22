using Microsoft.Extensions.Logging;
using NodaTime;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests.Services
{
    [Collection("Flight Ops configuration")]
    public class OperationalScheduleProviderTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpConnector _httpConnector;
        private readonly OperationalScheduleProvider _provider;
        private readonly ILogger<HttpConnector> _logger;

        public OperationalScheduleProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _provider = new OperationalScheduleProvider(_httpConnector);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task GetOperationalScheduleAsync_ReturnsAircraftData()
        {
            //Arrange
            var operationScheduleRequest = new Infrastructure.Models.OperationalSchedule.OperationScheduleRequest
            {
                AirlineCode = "VS",
                LocalDateFrom = LocalDateTime.FromDateTime(DateTime.Parse("2026-01-16T05:00")),
                LocalDateTo = LocalDateTime.FromDateTime(DateTime.Parse("2026-01-16T06:30")),
                OriginAirportCode = "LHR",
                FlightNumber = 103
            };

            //Act
            var result = await _provider.GetOperationalScheduleAsync(operationScheduleRequest);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Flights);
            Assert.NotNull(result.Flights.First().Aircraft);
            Assert.Equal("G-VTOM", result.Flights.First().Aircraft.Registration);
            Assert.Equal("Space Oddity", result.Flights.First().Aircraft.Name);

        }

        [Fact]
        public async Task GetOperationalScheduleAsync_ReturnsEmptyFlightsResponse()
        {
            ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/empty");

            var result = await _provider.GetOperationalScheduleAsync(new Infrastructure.Models.OperationalSchedule.OperationScheduleRequest
            {
                AirlineCode = "VS",
                FlightNumber = 7,
                OriginAirportCode = "LHR",
                DestinationAirportCode = "BGI"
            });

            Assert.NotNull(result);
            Assert.Empty(result.Flights);
        }

        [Fact]
        public async Task GetOperationalScheduleAsync_ThrowsForServiceFailure()
        {
            ConfigReader.Set(Common.Constants.Environment.FlightOpsUrl, "http://tempURL.org/error");

            await Assert.ThrowsAsync<HttpRequestException>(() => _provider.GetOperationalScheduleAsync(new Infrastructure.Models.OperationalSchedule.OperationScheduleRequest
            {
                AirlineCode = "VS",
                FlightNumber = 7
            }));
        }
    }
}
