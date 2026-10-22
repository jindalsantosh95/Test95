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
    public class OTPEmailCommandHandlerTests
    {
        private readonly Mock<ILogger<OTPEmailCommandHandler>> _mockLogger;
        private readonly OTPEmailCommandHandler _handler;
        private readonly Mock<IACMClient> _client;

        public OTPEmailCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger<OTPEmailCommandHandler>>();
            _client = new Mock<IACMClient>();
            _handler = new OTPEmailCommandHandler(_mockLogger.Object, _client.Object);
            EmailTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OTPEmailCommandHandler(null, _client.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenClientObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OTPEmailCommandHandler(_mockLogger.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullException_WhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public void Handler_ShouldReturnTrue_ForSuccessfulRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OTPEmailCommand>("EmailMS\\OTPEmailCommand");

            _client.Setup(_ => _.SendEmail(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var result = _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.IsType<bool>(result.Result);
            Assert.True(result.Result);
        }

        [Fact]
        public void Handler_ShouldReturnFalse_ForFailingtoConnecttoACM()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OTPEmailCommand>("EmailMS\\OTPEmailCommand");

            //Act
            var result = _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.IsType<bool>(result.Result);
            Assert.False(result.Result);
        }

        [Fact]
        public async Task Handler_ShouldThrowException_ForBadRequest()
        {
            //Arrange
            //Act
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }
    }
}
