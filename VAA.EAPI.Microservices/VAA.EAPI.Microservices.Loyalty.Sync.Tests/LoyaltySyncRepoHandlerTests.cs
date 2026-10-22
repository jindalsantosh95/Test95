using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.Loyalty.SyncRepository;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class LoyaltySyncRepoHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<LoyaltySyncRepoHandler>> _mockLoyaltySyncRepoLogger;

        public LoyaltySyncRepoHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLoyaltySyncRepoLogger = new Mock<ILogger<LoyaltySyncRepoHandler>>();
        }

        [Fact]
        public void NullLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltySyncRepoHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void NullMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltySyncRepoHandler(null, _mockLoyaltySyncRepoLogger.Object));
        }

        [Fact]
        public async Task SucceededRequest_ShouldReturnTrue()
        {
            // Arrange
            Mock<IMediator> mockMediator = new();
            mockMediator.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

            LoyaltySyncRepoHandler handler = new(mockMediator.Object, _mockLoyaltySyncRepoLogger.Object);
            LoyaltySyncRepoCommand request = new(new LoyaltySyncEventProcessCommand());

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task FailedRequest_ShouldReturnFalse()
        {
            // Arrange
            Mock<IMediator> mockMediator = new();
            mockMediator.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

            LoyaltySyncRepoHandler handler = new(mockMediator.Object, _mockLoyaltySyncRepoLogger.Object);
            LoyaltySyncRepoCommand request = new(new LoyaltySyncEventProcessCommand());

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsFailure);
        }
    }
}
