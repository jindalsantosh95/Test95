using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Email.Client;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Infrastructure.Email.Tests
{
    public class ACMClientTests
    {
        private readonly Mock<ILogger<ACMClient>> _logger;
        private readonly Mock<IHttpConnector> _mockHttp;
        private readonly Mock<ITokenHelper> _tokenHelper;
        private readonly ACMClient _acmClient;

        public ACMClientTests()
        {
            _logger = new Mock<ILogger<ACMClient>>();
            _mockHttp = new Mock<IHttpConnector>();
            _tokenHelper = new Mock<ITokenHelper>();
            _acmClient = new ACMClient(_logger.Object, _mockHttp.Object, _tokenHelper.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenloggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ACMClient(null, _mockHttp.Object, _tokenHelper.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenHttpClientFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ACMClient(_logger.Object, null, _tokenHelper.Object));
        }

        [Fact]
        public void ACMClient_ShouldReturnFalse_ForIncorrectRequestDetails()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<string>("Infrastructure.Email\\ACMEmailRequest");

            //Act
            var result = _acmClient.SendEmail(request);

            //Assert
            Assert.False(result.GetAwaiter().GetResult());
        }

        [Fact]
        public async Task ACMClient_ShouldReturnTrue_ForISuccessfulRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<string>("Infrastructure.Email\\ACMEmailRequest");

            _mockHttp.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>()))
                     .ReturnsAsync(new HttpResponseMessage()
                     {
                         StatusCode = HttpStatusCode.OK
                     })
                     .Verifiable();

            //Act
            var result = await _acmClient.SendEmail(request);

            //Assert
            Assert.True(result);
        }

    }
}
