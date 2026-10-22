using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public sealed class VirginPointsServiceHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly ITokenHelper _tokenHelper;
        private readonly VirginPointsServiceHandler _pointsService;

        public VirginPointsServiceHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _tokenHelper = new TokenHelper(_httpConnector);
            _pointsService = new VirginPointsServiceHandler(_httpConnector, _tokenHelper);

            VRedCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenHttpConnectorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new VirginPointsServiceHandler(null, _tokenHelper));

            Assert.Equal("http", exception.ParamName);
        }

        [Fact]
        public async Task GetVirginPoints_ReturnsSuccessfulResponse()
        {
            // Arrange
            var request = new VirginPointsServiceQuery("64372184");
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _pointsService.Handle(request, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("1000", result.Current);
            Assert.Contains("500", result.Usable);
        }

    }
}