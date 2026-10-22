using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Tests
{
    public class SkyTeamEligibiltyRepoCommandHandlerTests
    {
        private readonly Mock<ISkyTeamRetro> _skyTeamRetro;
        private readonly Mock<ILogger<SkyTeamEligibiltyRepoCommandHandler>> _mockLogger;
        private readonly SkyTeamEligibiltyRepoCommandHandler _handler;

        public SkyTeamEligibiltyRepoCommandHandlerTests()
        {
            _skyTeamRetro = new Mock<ISkyTeamRetro>();
            _mockLogger = new Mock<ILogger<SkyTeamEligibiltyRepoCommandHandler>>();
            _handler = new SkyTeamEligibiltyRepoCommandHandler(_mockLogger.Object, _skyTeamRetro.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SkyTeamEligibiltyRepoCommandHandler(null, _skyTeamRetro.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenSkyTeamRetroIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SkyTeamEligibiltyRepoCommandHandler(_mockLogger.Object, null));
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
            var request = TestDataProvider.GetTestData<SkyTeamEligibiltyRepoCommand>("SkyMS//CreateSkyTeamRetroRequest");

            var eligibilityCheckcRepoSuccessResponse = TestDataProvider.GetTestData<OnlineRetroCreditingResponse>("SkyMS//SkyRetroEligibilityCheckSuccessResponse");

            _skyTeamRetro.Setup(x => x.SkyTeamRetroEligibiltycheck(It.IsAny<OnlineRetroCreditRequestType>())).ReturnsAsync(eligibilityCheckcRepoSuccessResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsEligible);
            Assert.Equal("000", result.EligibilityDenialCode);
            Assert.IsType<SkyClaimResponse>(result.SkyClaimResponse);
            Assert.Equal("ST4224113475558091776", result.SkyClaimResponse.UniqueIdentificationNumber);
        }

        [Fact]
        public async Task Handler_ShouldReturnFailure_ForIneligibleRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyTeamEligibiltyRepoCommand>("SkyMS//CreateSkyTeamRetroRequest");

            var eligibilityCheckcRepoFailureResponse = TestDataProvider.GetTestData<OnlineRetroCreditingResponse>("SkyMS//SkyRetroEligibilityCheckFailureResponse");

            _skyTeamRetro.Setup(x => x.SkyTeamRetroEligibiltycheck(It.IsAny<OnlineRetroCreditRequestType>())).ReturnsAsync(eligibilityCheckcRepoFailureResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Equal("101", result.EligibilityDenialCode);
            Assert.IsType<SkyClaimResponse>(result.SkyClaimResponse);
            Assert.Equal("612", result.SkyClaimResponse.OperatingFlightNumber);
        }
    }
}
