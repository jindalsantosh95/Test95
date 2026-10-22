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
    public sealed class FlightCrewRosterServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<FlightCrewRosterService> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IAwsS3Buckets _awsS3Buckets;

        public FlightCrewRosterServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<FlightCrewRosterService>.Instance;
            _httpConnector = new HttpConnector(logger, httpClientFactoryFixture.Current);
            _awsS3Buckets = new S3Buckets(new S3BucketService(nameof(FlightCrewRosterService)));

            ConfigReader.Set("RetailTherapy:FlightMs:CrewRosterUri", "https://tempuri.org/FlightCrewRoster");
            ConfigReader.Set("RetailTherapy:FlightMs:ScheduleUri", "https://tempuri.org/FlightSchedule");
            ConfigReader.Set("RetailTherapy:FlightMs:DetailsUri", "https://tempuri.org/FlightDetail");
            ConfigReader.Set("RetailTherapy:CrewMs:OcpSubsctiption", Guid.NewGuid().ToString());
            ConfigReader.Set("RetailTherapy:FlightMs:OcpSubsctiption", Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task FlightCrewRosterService()
        {
            var flightCrewRosterService = new FlightCrewRosterService(_logger, _httpConnector, _awsS3Buckets);

            await flightCrewRosterService.PublishAsync();

            var awsS3BucketService = (S3BucketService)_awsS3Buckets.First();

            Assert.Single(awsS3BucketService.Items);

            var lines = UTF8Encoding.UTF8.GetString(awsS3BucketService.Items.First().Value)
                                         .Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
            var data = lines[1].Split(';', System.StringSplitOptions.RemoveEmptyEntries);

            // check header and single record has been parsed
            Assert.Equal(2, lines.Length);

            // check header column and row column count match
            Assert.Equal(lines[0].Split(';', System.StringSplitOptions.RemoveEmptyEntries).Length, data.Length);

            // check lines are what we expect to be
            Assert.Equal("FLIGHT_ID;FLIGHT_NUMBER;DEPARTURE_DATE;CREW_ID;CREW_NAME;CREW_ROLE", lines[0]);
            Assert.Equal("202506152215104", data[0]);
            Assert.Equal("2025-06-15", data[2]);
            Assert.Equal("DOE John", data[4]);
        }
    }
}
