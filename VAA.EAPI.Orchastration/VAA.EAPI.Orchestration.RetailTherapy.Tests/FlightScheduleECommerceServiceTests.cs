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
    public sealed class FlightScheduleECommerceServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<FlightScheduleECommerceService> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IAwsS3Buckets _awsS3Buckets;

        public FlightScheduleECommerceServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<FlightScheduleECommerceService>.Instance;
            _httpConnector = new HttpConnector(logger, httpClientFactoryFixture.Current);
            _awsS3Buckets = new S3Buckets(new S3BucketService(nameof(FlightScheduleECommerceService)));

            ConfigReader.Set("RetailTherapy:ReferenceMs:Uri", "https://tempuri.org/Reference");
            ConfigReader.Set("RetailTherapy:FlightMs:DetailsUri", "https://tempuri.org/FlightDetail");
            ConfigReader.Set("RetailTherapy:CrewMs:OcpSubsctiption", Guid.NewGuid().ToString());
            ConfigReader.Set("RetailTherapy:FlightMs:OcpSubsctiption", Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task FlightScheduleECommerceService()
        {
            var flightScheduleConsolidatedService = new FlightScheduleECommerceService(_logger, _httpConnector, _awsS3Buckets);

            await flightScheduleConsolidatedService.PublishAsync();

            var awsS3BucketService = (S3BucketService)_awsS3Buckets.First();

            Assert.Single(awsS3BucketService.Items);

            var lines = UTF8Encoding.UTF8.GetString(awsS3BucketService.Items.First().Value)
                                         .Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
            var data = lines[1].Split(',');

            // check header and single record has been parsed
            Assert.Equal(2, lines.Length);

            // check header column and row column count match
            Assert.Single(lines.Select(x => x.Split(';').Length).Distinct());

            // check lines are what we expect to be
            Assert.Equal("primaryFlightNumber,airlineCode,airlineName,scheduledDepartureDate,scheduledDepartureDateTime,scheduledArrivalDate,scheduledArrivalDateTime,arrivalAirportCode,arrivalAirportName,arrivalCountryName,arrivalCityName,departureAirportCode,departureAirportName,departureCountryName,departureCityName,direction", lines[0]);
            Assert.Equal("VS104", data[0]); //primaryFlightNumber
            Assert.Equal("LHR", data[7]); // arrivalAirportCode
            Assert.Equal("ATL", data[11]); // departureAirportCode
            Assert.Equal("2025-06-15", data[3]); // scheduledDepartureDate
            Assert.Equal("2025-06-15T18:15:00-04:00", data[4]); // scheduledDepartureDateTime
        }
    }
}
