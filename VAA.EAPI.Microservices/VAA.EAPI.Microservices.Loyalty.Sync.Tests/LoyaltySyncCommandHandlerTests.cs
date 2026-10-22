using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.Loyalty.SyncRepository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class LoyaltySyncCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<LoyaltySyncCommandHandler>> _mockLogger;
        private readonly LoyaltySyncCommandHandler _handler;

        public LoyaltySyncCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMediator.Setup(m => m.Send(It.IsAny<LoyaltySyncRepoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);
            _mockLogger = new Mock<ILogger<LoyaltySyncCommandHandler>>();
            _handler = new LoyaltySyncCommandHandler(_mockMediator.Object, _mockLogger.Object);

            LoyaltySyncTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltySyncCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new LoyaltySyncCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task Handle_GoodRequest_ShouldReturnSuccess()
        {
            // Arrange            
            var request = TestDataProvider.GetTestData<LoyaltySyncEventProcessCommand>("LoyaltySync.GoodRequest");
            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_GoodRequest_ShouldReturnSuccess_LastNameSplChar()
        {
            // Arrange            
            var request = TestDataProvider.GetTestData<LoyaltySyncEventProcessCommand>("LoyaltySync.GoodRequest");
            request.Data.LastName = "Rose-Test";

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }
    }
}
