using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Email.Tests
{
    public class AdobeEmailCommandHandlerTests
    {
        private readonly Mock<ILogger<AdobeEmailCommandHandler>> _mockLogger;
        private readonly AdobeEmailCommandHandler _handler;
        private readonly Mock<IACMClient> _client;

        public AdobeEmailCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger<AdobeEmailCommandHandler>>();
            _client = new Mock<IACMClient>();
            _handler = new AdobeEmailCommandHandler(_mockLogger.Object, _client.Object);
            EmailTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AdobeEmailCommandHandler(null, _client.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenClientObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AdobeEmailCommandHandler(_mockLogger.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullException_WhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnTrue_ForSuccessfulRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AdobeEmailCommand>("EmailMS\\AdobeEmailCommandRequest");

            _client.Setup(_ => _.SendEmail(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.IsType<bool>(result.Value);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnFalse_ForFailingtoConnecttoACM()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AdobeEmailCommand>("EmailMS\\AdobeEmailCommandRequest");

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.IsType<bool>(result.Value);
            Assert.False(result.Value);
        }
    }
}
