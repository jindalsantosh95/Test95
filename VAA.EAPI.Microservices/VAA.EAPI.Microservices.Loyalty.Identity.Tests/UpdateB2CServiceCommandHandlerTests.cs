using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class UpdateB2CServiceCommandHandlerTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttp;
        private readonly Mock<ILogger<UpdateB2CServiceCommandHandler>> _mockLogger;
        private readonly UpdateB2CServiceCommandHandler _handler;

        public UpdateB2CServiceCommandHandlerTests()
        {
            _mockHttp = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<UpdateB2CServiceCommandHandler>>();
            _handler = new UpdateB2CServiceCommandHandler(_mockHttp.Object, _mockLogger.Object);
            IdentityTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateB2CServiceCommandHandler(_mockHttp.Object, null));
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateB2CServiceCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task Handler_ShouldReturnTrue_ForSuccessfulUpdate()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateB2CServiceCommand>("IdentityMS\\UpdateB2CCommandRequest");

            var httpResponseMessageContent = TestDataProvider.GetTestData<string>("IdentityMS\\HttpResponseMessage");

            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(httpResponseMessageContent)
               })
               .Verifiable();

            HttpClient httpClient = new HttpClient(handlerMock.Object);
            _mockHttp.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_ForUnsuccessfulUpdate()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateB2CServiceCommand>("IdentityMS\\UpdateB2CCommandRequest");

            var httpResponseMessageContent = TestDataProvider.GetTestData<string>("IdentityMS\\HttpResponseMessage");

            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest,
                   Content = new StringContent(httpResponseMessageContent)
               })
               .Verifiable();

            HttpClient httpClient = new HttpClient(handlerMock.Object);
            _mockHttp.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<bool>(result);
            Assert.False(result);
        }
    }
}
