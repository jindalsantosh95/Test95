using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sky.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Tests
{
    public class CreateSkyTeamRetroCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CreateSkyTeamRetroCommandHandler>> _mockLogger;
        private readonly CreateSkyTeamRetroCommandHandler _handler;

        public CreateSkyTeamRetroCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CreateSkyTeamRetroCommandHandler>>();
            _handler = new CreateSkyTeamRetroCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CreateSkyTeamRetroCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CreateSkyTeamRetroCommandHandler(_mockMediator.Object, null));
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
            var request = TestDataProvider.GetTestData<CreateSkyTeamRetroCommand>("SkyMS//CreateSkyTeamRetroRequest");

            var eligibilityCheckcRepoTrueResponse = TestDataProvider.GetTestData<CheckEligibilityResponse>("SkyMS//EligibilityCheckResponse");

            _mockMediator.Setup(s => s.Send(It.IsAny<SkyTeamEligibiltyRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(eligibilityCheckcRepoTrueResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Errors);
            Assert.Equal(Status.OK, result.Status);
            Assert.IsType<CheckEligibilityResponse>(result.Value);
            Assert.True(result.Value.IsEligible);
            Assert.Equal("000", result.Value.EligibilityDenialCode);
            Assert.IsType<SkyClaimResponse>(result.Value.SkyClaimResponse);
            Assert.Equal("ST67643563783743", result.Value.SkyClaimResponse.UniqueIdentificationNumber);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForIneligibleRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateSkyTeamRetroCommand>("SkyMS//CreateSkyTeamRetroRequest");

            var eligibilityCheckcRepoFalseResponse = TestDataProvider.GetTestData<CheckEligibilityResponse>("SkyMS//EligibilityCheckResponse");
            eligibilityCheckcRepoFalseResponse.IsEligible = false;
            eligibilityCheckcRepoFalseResponse.EligibilityDenialCode = "104";

            _mockMediator.Setup(s => s.Send(It.IsAny<SkyTeamEligibiltyRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(eligibilityCheckcRepoFalseResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Value);
            Assert.IsType<Response<CheckEligibilityResponse>>(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("S104", result.Errors.First().Code);
            Assert.Equal("Passenger not found on this flight by the operating airline.", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForTechnicalIssues()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateSkyTeamRetroCommand>("SkyMS//CreateSkyTeamRetroRequest");
            request.TicketInfo.ScheduledFlightSegmentDepartureDate = "20240313";

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Value);
            Assert.IsType<Response<CheckEligibilityResponse>>(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("G009", result.Errors.First().Code);
            Assert.Equal("There was an unexpected error.", result.Errors.First().Message);
        }
    }
}
