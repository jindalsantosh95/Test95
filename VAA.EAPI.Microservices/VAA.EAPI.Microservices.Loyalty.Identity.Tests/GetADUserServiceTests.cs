using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Text.Json;
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

    [Collection("EnvironmentDependentTests")]
    public sealed class GetAdUserServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly IGetAdUserDetailsService _userDetailsService;

        public GetAdUserServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _userDetailsService = new GetAdUserDetailsService(_httpConnector);

            IdentityTestsCommon.CreateTestEnvironmentSettings();

            ConfigReader.Set("GraphAPIUrl", "https://tempuri.org/");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpConnectorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new GetAdUserDetailsService(null));

            Assert.Equal("httpConnector", exception.ParamName);
        }

        [Fact]
        public async Task GetDetailsByEmail_WithValidParameters_ReturnsSuccessfulResponse()
        {
            // Arrange
            var api = "/usersEmail";
            var verifyB2CServiceQuery = "$filter=identities/any(c:c/issuerAssignedIdeq'deventratest@test.com'andc/issuereq'entratenant.onmicrosoft.com')";
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _userDetailsService.GetDetailsFromGraphApi(api, verifyB2CServiceQuery, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\"id\":", result);
            Assert.Contains("\"displayName\":", result);
            Assert.Contains("deventratest@test.com", result);
            Assert.Contains("Test User", result);
        }

        [Fact]
        public async Task GetDetailsByEmail_WithEmptyQuery_ReturnsSuccessfulResponse()
        {
            // Arrange
            var api = "/users";
            var verifyB2CServiceQuery = "";
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _userDetailsService.GetDetailsFromGraphApi(api, verifyB2CServiceQuery, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\"value\":", result);
            Assert.Contains("User One", result);
            Assert.Contains("User Two", result);
        }

        [Fact]
        public async Task GetDetailsByEmail_WithNullQuery_ReturnsSuccessfulResponse()
        {
            // Arrange
            var api = "/usersEmpty";
            string verifyB2CServiceQuery = null;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _userDetailsService.GetDetailsFromGraphApi(api, verifyB2CServiceQuery, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\"value\":", result);
        }
    }
}