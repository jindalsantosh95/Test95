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
    public class WelcomeEmailCommandHandlerTests
    {
        private readonly Mock<ILogger<WelcomeEmailCommandHandler>> _mockLogger;
        private readonly WelcomeEmailCommandHandler _handler;
        private readonly Mock<IACMClient> _client;

        public WelcomeEmailCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger<WelcomeEmailCommandHandler>>();
            _client = new Mock<IACMClient>();
            _handler = new WelcomeEmailCommandHandler(_mockLogger.Object, _client.Object);
            EmailTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new WelcomeEmailCommandHandler(null, _client.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenClientObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new WelcomeEmailCommandHandler(_mockLogger.Object, null));
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
            var request = TestDataProvider.GetTestData<WelcomeEmailEventProcessCommand>("EmailMS\\WelcomeEmailEventProcessCommandRequest");

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
            var request = TestDataProvider.GetTestData<WelcomeEmailEventProcessCommand>("EmailMS\\WelcomeEmailEventProcessCommandRequest");

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
