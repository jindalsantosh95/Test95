using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Applications.Command.ShopsAway;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Security.Tests.Applications
{
    public class ShopsAwayTokenCommandHandlerTests
    {
        private readonly Mock<ILogger<ShopsAwayTokenCommandHandler>> _mockLogger;
        private readonly ShopsAwayTokenCommandHandler _handler;

        public ShopsAwayTokenCommandHandlerTests()
        {
            _mockLogger = new Mock<ILogger<ShopsAwayTokenCommandHandler>>();
            _handler = new ShopsAwayTokenCommandHandler(_mockLogger.Object);

            ConfigReader.Set("ShopsAwayAESSecret", "ShopsAwayAESSecret");
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new ShopsAwayTokenCommandHandler(null));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullException_WhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnResponse_ForSuccessfulRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<ShopsAwayTokenCommand>("SecurityMS\\ShopsAwayTokenCommandRequest");

            ConfigReader.Set(Constants.Environment.ShopsAwayAESSecret, "2316-9278-8ca4-e17e-7037-cb10-5833-9947");

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(result.Value);
            Assert.Null(result.Errors);
        }

        [Fact]
        public async Task Handler_ShouldThrowException_ForInvalidRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<ShopsAwayTokenCommand>("SecurityMS\\ShopsAwayTokenCommandRequest");

            //Act
            request.MemberDetails = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}
