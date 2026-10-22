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
    public sealed class CrewMasterServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<CrewMasterService> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly IAwsS3Buckets _awsS3Buckets;

        public CrewMasterServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CrewMasterService>.Instance;
            _httpConnector = new HttpConnector(logger, httpClientFactoryFixture.Current);
            _awsS3Buckets = new S3Buckets(new S3BucketService(nameof(CrewMasterService)));

            ConfigReader.Set("RetailTherapy:CrewMs:Uri", "https://tempuri.org/CrewMaster");
            ConfigReader.Set("RetailTherapy:CrewMs:OcpSubsctiption", Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task CrewMasterService()
        {
            var crewMasterService = new CrewMasterService(_logger, _httpConnector, _awsS3Buckets);

            await crewMasterService.PublishAsync();

            var awsS3BucketService = (S3BucketService)_awsS3Buckets.First();

            Assert.Single(awsS3BucketService.Items);

            var lines = UTF8Encoding.UTF8.GetString(awsS3BucketService.Items.First().Value)
                                         .Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);

            // check header and single record has been parsed
            Assert.Equal(2, lines.Length);

            //check header column and row column count match
            Assert.Single(lines.Select(x => x.Split(',').Length).Distinct());

            // check lines are what we expect to be
            Assert.Equal("CrewID;CrewFirstName;CrewLastName;CrewMailAddress;CrewBase", lines[0]);
            Assert.Equal("012345", lines[1].Split(';')[0]);
            Assert.Equal("John", lines[1].Split(';')[1]);
            Assert.Equal("Doe", lines[1].Split(';')[2]);
            Assert.Equal("johndoe@tempuri.org", lines[1].Split(';')[3]);
        }
    }
}
