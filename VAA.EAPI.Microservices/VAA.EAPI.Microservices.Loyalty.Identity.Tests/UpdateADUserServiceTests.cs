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
    public sealed class UpdateAdUserServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _loggerHttp;
        private readonly ILogger<UpdateAdUserService> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly IUpdateAdUserService _updateAdUserService;

        public UpdateAdUserServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _loggerHttp = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<UpdateAdUserService>.Instance;
            _httpConnector = new HttpConnector(_loggerHttp, httpClientFactoryFixture.Current);
            _updateAdUserService = new UpdateAdUserService(_httpConnector, _logger);

            IdentityTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpConnectorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new UpdateAdUserService(null, _logger));

            Assert.Equal("httpConnector", exception.ParamName);
        }

        [Fact]
        public async Task UpadteEmailAuthMethod_WhereNoneCurrentlyExitsts_ReturnsSuccessfulResponse()
        {
            // Arrange
            var oid = "0a58c1c7-821e-40b6-84e0-71ee9a36bc59";
            var url = $"/users/{oid}/authentication/emailMethods";
            string verifyB2CServiceQuery = null;
            string content = string.Empty;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _updateAdUserService.UpdateAdUser(url, verifyB2CServiceQuery, content, HttpMethod.Post, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsB2COperationSuccess);
        }

        [Fact]
        public async Task GetDetailsByEmail_WithNullQuery_ReturnsSuccessfulResponse()
        {
            // Arrange
            var oid = "0a58c1c7-821e-40b6-84e0-71ee9a36bc58";
            var url = $"/users/{oid}/authentication/emailMethods";
            string verifyB2CServiceQuery = null;
            string content = string.Empty;
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _updateAdUserService.UpdateAdUser(url, verifyB2CServiceQuery, content, HttpMethod.Post, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsB2COperationSuccess);
        }

        [Fact]
        public async Task PatchDetailsEmailMFA_ReturnsSuccessfulResponse()
        {
            // Arrange
            var api = "/users/8ece9bd0-7a8e-4e7b-9083-bb808b4e8bb0/authentication/emailMethods/c71f9f67-917a-4490-9a2c-d60520fb63ae";

            var payload = new
            {
                emailAddress = "testemail.com"
            };

            string content = JsonSerializer.Serialize(payload);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _updateAdUserService.UpdateMFADetailsFromGraphApi(api, content, cancellationToken);

            // Assert
            Assert.True(result);
        }
    }
}