using Aliencube.AdalWrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class IdentityServiceHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<IdentityServiceHandler> _serviceLogger;

        public IdentityServiceHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _serviceLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<IdentityServiceHandler>.Instance;

            //TestEnvironmentSettings
            IdentityTestsCommon.CreateTestEnvironmentSettings();

        }

        [Fact]
        public void NullLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new IdentityServiceHandler(_httpConnector, null));
        }

        [Fact]
        public void NullHttpClient_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new IdentityServiceHandler(null, _serviceLogger));
        }

        [Fact]
        public async Task SuccessfulEnrollment_ShouldReturnTrue()
        {
            //Arrange
            var httpResponseMessageContent = TestDataProvider.GetTestData<string>("IdentityMS\\HttpResponseMessage");

            IdentityServiceHandler handler = new(_httpConnector, _serviceLogger);
            var request = new IdentityServiceCommand("/IdenityServiceCommand", "xyz");

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.True(result);

        }

        [Fact]
        public async Task UnsuccessfulEnrollment_ShouldReturnFalse()
        {
            //Arrange
            var httpResponseMessageContent = TestDataProvider.GetTestData<string>("IdentityMS\\HttpResponseMessage");

            IdentityServiceHandler handler = new(_httpConnector, _serviceLogger);
            var request = new IdentityServiceCommand("abc", "xyz");

            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.False(result);

        }
    }
}
