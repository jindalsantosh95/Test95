using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Service;
using Xunit;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Tests
{
    public sealed class FlightsServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<FlightsService> _flightsLogger;
        private readonly HttpConnector _httpConnector;

        public FlightsServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _flightsLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<FlightsService>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);

            ConfigReader.Set("Aims:APIM:Url", "https://Testresources.virginatlantic.com/aims");
            ConfigReader.Set("Aims:Username", "000000000000");
            ConfigReader.Set("Aims:Password", "000000000000");
            ConfigReader.Set("Aims:APIM:OcpSubscriptionKey", "00000000-0000-0000-0000-000000000000");
        }

        [Fact]
        public async Task FlightServiceRoster()
        {
            var flightsService = new FlightsService(_httpConnector, _flightsLogger);

            var response = await flightsService.RosterAsync(new FlightCrewRosterRequest()
            {

            });

            Assert.NotNull(response.Value);
            Assert.NotNull(response.Value.Value);

            var flightCrewRosterResponse = response.Value.Value;

            Assert.True(flightCrewRosterResponse.Any());
        }

        [Fact]
        public async Task FlightServiceDetail()
        {
            var flightsService = new FlightsService(_httpConnector, _flightsLogger);

            var response = await flightsService.DetailAsync(new FlightDetailRequest()
            {

            });

            Assert.NotNull(response.Value);
            Assert.NotNull(response.Value.Value);

            var flightCrewRosterResponse = response.Value.Value;

            Assert.True(flightCrewRosterResponse.Any());
        }

        [Fact]
        public async Task FlightServiceSchedule()
        {
            var flightsService = new FlightsService(_httpConnector, _flightsLogger);

            var response = await flightsService.ScheduleAsync(new FlightScheduleRequest()
            {

            });

            Assert.NotNull(response.Value);
            Assert.NotNull(response.Value.Value);

            var flightCrewRosterResponse = response.Value.Value;

            Assert.True(flightCrewRosterResponse.Any());
        }
    }
}
