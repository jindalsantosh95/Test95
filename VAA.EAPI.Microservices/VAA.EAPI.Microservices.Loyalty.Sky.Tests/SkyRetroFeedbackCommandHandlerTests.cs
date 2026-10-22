using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sky.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Tests
{
    public class SkyRetroFeedbackCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<SkyRetroFeedbackCommandHandler>> _mockLogger;
        private readonly SkyRetroFeedbackCommandHandler _handler;
        public SkyRetroFeedbackCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<SkyRetroFeedbackCommandHandler>>();
            _handler = new SkyRetroFeedbackCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SkyRetroFeedbackCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new SkyRetroFeedbackCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowException_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodUpdateRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyRetroFeedbackCommand>("SkyMS//SkyRetroFeedbackCommandRequest");

            var skyUpdateRepoSuccessResponse = TestDataProvider.GetTestData<SkyOnlineRetroFeedbackResponse>("SkyMS//SkyUpdateResponse");

            _mockMediator.Setup(s => s.Send(It.IsAny<SkyRetroFeedbackRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(skyUpdateRepoSuccessResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Errors);
            Assert.Equal(Status.OK, result.Status);
            Assert.IsType<SkyOnlineRetroFeedbackResponse>(result.Value);
            Assert.True(result.Value.IsSuccess);
            Assert.Equal("999", result.Value.EligibilityDenialCode);
            Assert.Equal("ST4434357905628432344", result.Value.UniqueIdentificationNumber);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForFailedUpdateRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyRetroFeedbackCommand>("SkyMS//SkyRetroFeedbackCommandRequest");

            var skyUpdateRepoFailureResponse = TestDataProvider.GetTestData<SkyOnlineRetroFeedbackResponse>("SkyMS//SkyUpdateResponse");
            skyUpdateRepoFailureResponse.EligibilityDenialCode = "997";
            skyUpdateRepoFailureResponse.IsSuccess = false;

            _mockMediator.Setup(s => s.Send(It.IsAny<SkyRetroFeedbackRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(skyUpdateRepoFailureResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Value);
            Assert.IsType<Response<SkyOnlineRetroFeedbackResponse>>(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("S997", result.Errors.First().Code);
            Assert.Equal("Technical issue encountered.", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForTechnicalIssues()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyRetroFeedbackCommand>("SkyMS//SkyRetroFeedbackCommandRequest");

            _mockMediator.Setup(s => s.Send(It.IsAny<SkyRetroFeedbackRepoCommand>(), It.IsAny<CancellationToken>())).Throws<ArgumentNullException>();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Value);
            Assert.IsType<Response<SkyOnlineRetroFeedbackResponse>>(result);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.IsType<List<ErrorDetails>>(result.Errors);
            Assert.Equal("G009", result.Errors.First().Code);
            Assert.Equal("There was an unexpected error.", result.Errors.First().Message);
        }
    }
}
