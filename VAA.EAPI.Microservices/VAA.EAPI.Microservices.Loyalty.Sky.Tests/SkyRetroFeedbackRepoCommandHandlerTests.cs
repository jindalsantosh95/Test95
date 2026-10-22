using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Tests
{
    public class SkyRetroFeedbackRepoCommandHandlerTests
    {
        private readonly Mock<ISkyTeamRetro> _skyTeamRetro;
        private readonly Mock<ILogger<SkyRetroFeedbackRepoCommandHandler>> _mockLogger;
        private readonly SkyRetroFeedbackRepoCommandHandler _handler;
        public SkyRetroFeedbackRepoCommandHandlerTests()
        {
            _skyTeamRetro = new Mock<ISkyTeamRetro>();
            _mockLogger = new Mock<ILogger<SkyRetroFeedbackRepoCommandHandler>>();
            _handler = new SkyRetroFeedbackRepoCommandHandler(_mockLogger.Object, _skyTeamRetro.Object);

        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SkyRetroFeedbackRepoCommandHandler(null, _skyTeamRetro.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenSkyTeamRetroIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SkyRetroFeedbackRepoCommandHandler(_mockLogger.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowException_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyRetroFeedbackRepoCommand>("SkyMS//SkyRetroFeedbackCommandRequest");

            var updateSuccessResponse = TestDataProvider.GetTestData<OnlineRetroCreditFeedbackResponse>("SkyMS//SkyRetroUpdateSuccessResponse");

            _skyTeamRetro.Setup(x => x.SkyTeamRetroFeedback(It.IsAny<OnlineFeedbackRequestType>())).ReturnsAsync(updateSuccessResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("999", result.EligibilityDenialCode);
            Assert.Equal("ST4224113475558091776", result.UniqueIdentificationNumber);
        }

        [Fact]
        public async Task Handler_ShouldReturnFailure_ForFailedUpdateRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyRetroFeedbackRepoCommand>("SkyMS//SkyRetroFeedbackCommandRequest");

            var updateFailedResponse = TestDataProvider.GetTestData<OnlineRetroCreditFeedbackResponse>("SkyMS//SkyRetroUpdateSuccessResponse");
            updateFailedResponse.OnlineFeedbackResponse.EligibilityDenialCode = "997";

            _skyTeamRetro.Setup(x => x.SkyTeamRetroFeedback(It.IsAny<OnlineFeedbackRequestType>())).ReturnsAsync(updateFailedResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("997", result.EligibilityDenialCode);
            Assert.Equal("ST4224113475558091776", result.UniqueIdentificationNumber);
        }
    }
}
