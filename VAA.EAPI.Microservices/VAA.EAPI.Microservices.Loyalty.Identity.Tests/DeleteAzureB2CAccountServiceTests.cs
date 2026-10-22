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
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public sealed class DeleteAzureB2CAccountServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly ILogger<DeleteB2CAccountService> _serviceLogger;
        private readonly IHttpConnector _httpConnector;
        private readonly IDeleteB2CAccountService _deleteB2CAccountService;

        public DeleteAzureB2CAccountServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _serviceLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DeleteB2CAccountService>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _deleteB2CAccountService = new DeleteB2CAccountService(_httpConnector, _serviceLogger);

            IdentityTestsCommon.CreateTestEnvironmentSettings();

            ConfigReader.Set("GraphAPIUrl", "https://tempuri.org/");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpConnectorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DeleteB2CAccountService(null, _serviceLogger));

            Assert.Equal("httpConnector", exception.ParamName);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DeleteB2CAccountService(_httpConnector, null));
        }

        [Fact]
        public async Task DeleteB2CAccount_WithValidCustomerId_ReturnsSuccessfulResponse()
        {
            // Arrange
            var customerId = "50f758a2-817d-499e-ad6f-4d292b80a64c";

            // Act
            var result = await _deleteB2CAccountService.DeleteB2CAccount(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsB2COperationSuccess);
        }

        [Fact]
        public async Task DeleteB2CAccount_WithInvalidCustomerId_ReturnsFalseResponse()
        {
            // Arrange
            var customerId = "50f758a2-817d-499e-ad6f-4d292b80a64d";

            // Act
            var result = await _deleteB2CAccountService.DeleteB2CAccount(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsB2COperationSuccess);
        }
    }
}