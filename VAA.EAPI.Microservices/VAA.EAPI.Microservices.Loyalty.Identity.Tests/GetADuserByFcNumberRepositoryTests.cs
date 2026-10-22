using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class GetAdUserByFcNumberRepositoryTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly IGetAdUserDetailsService _getUserDetailsService;
        private readonly IGetAdUserByFcNumberRepository _getADuserByFcNumberRepository;
        private readonly ILogger<GetAdUserByFcNumberRepository> _loggerRepository;

        public GetAdUserByFcNumberRepositoryTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _loggerRepository = Microsoft.Extensions.Logging.Abstractions.NullLogger<GetAdUserByFcNumberRepository>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _getUserDetailsService = new GetAdUserDetailsService(_httpConnector);
            _getADuserByFcNumberRepository = new GetAdUserByFcNumberRepository(_getUserDetailsService, _loggerRepository);

            IdentityTestsCommon.CreateTestEnvironmentSettings();

            ConfigReader.Set("GraphAPIUrl", "https://tempuri.org/");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpConnectorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new GetAdUserByFcNumberRepository(null, _loggerRepository));

            Assert.Equal("getUserDetailsService", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new GetAdUserByFcNumberRepository(_getUserDetailsService ,null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task GetDetailsByFcNumber_WithValidParameters_ReturnsSuccessfulResponse()
        {
            //Arrange
            ConfigReader.Set("GraphAPIUrl", "https://tempuri.org/");

            // Act
            var result = await _getADuserByFcNumberRepository.GetCustomerIdByFcNumber(1501133589, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("12345678-1234-1234-1234-123456789012", result);
        }
    }
}