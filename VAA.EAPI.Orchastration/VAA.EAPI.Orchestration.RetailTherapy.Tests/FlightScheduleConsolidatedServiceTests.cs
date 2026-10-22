using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Aws;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Aws;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Orchestration.RetailTherapy.Tests
{
    public sealed class FlightScheduleConsolidatedServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<FlightScheduleConsolidatedService> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IAwsS3Buckets _awsS3Buckets;

        public FlightScheduleConsolidatedServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<FlightScheduleConsolidatedService>.Instance;
            _httpConnector = new HttpConnector(logger, httpClientFactoryFixture.Current);
            _awsS3Buckets = new S3Buckets(new S3BucketService(nameof(FlightScheduleConsolidatedService)));

            ConfigReader.Set("RetailTherapy:FlightMs:DetailsUri", "https://tempuri.org/FlightDetail");
            ConfigReader.Set("RetailTherapy:CrewMs:OcpSubsctiption", Guid.NewGuid().ToString());
            ConfigReader.Set("RetailTherapy:FlightMs:OcpSubsctiption", Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task FlightScheduleConsolidatedService()
        {
            var flightScheduleConsolidatedService = new FlightScheduleConsolidatedService(_logger, _httpConnector, _awsS3Buckets);

            await flightScheduleConsolidatedService.PublishAsync();

            var awsS3BucketService = (S3BucketService)_awsS3Buckets.First();

            Assert.Single(awsS3BucketService.Items);

            var lines = UTF8Encoding.UTF8.GetString(awsS3BucketService.Items.First().Value)
                                         .Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
            var data = lines[1].Split(';');

            // check header and single record has been parsed
            Assert.Equal(2, lines.Length);

            // check header column and row column count match
            Assert.Single(lines.Select(x => x.Split(';').Length).Distinct());

            // check lines are what we expect to be
            Assert.Equal("LegID;AirlineCode;FlightNumber;ScheduledOriginAirportCode;ScheduledDestinationAirportCode;ScheduledDepartureDateTime;ScheduledArrivalDateTime;AircraftType;AircraftRegistration;AircraftCapacity;LinkedFlightNumber", lines[0]);
            Assert.Equal("202506152215104", data[0]);
            Assert.Equal("G-VDOT", data[8]);
            Assert.Equal("322", data[9]);
        }
    }
}
