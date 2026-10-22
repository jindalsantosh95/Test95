using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class IdentityCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<IdentityCommandHandler>> _mockLogger;
        private readonly IdentityCommandHandler _handler;

        public IdentityCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            // Assumes the SEND method always returns TRUE (The actual response is not captured)
            _mockMediator.Setup(m => m.Send(It.IsAny<IdentityRepoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            _mockLogger = new Mock<ILogger<IdentityCommandHandler>>();
            _handler = new IdentityCommandHandler(_mockMediator.Object, _mockLogger.Object);

        }

        [Fact]
        public void EmptyLogger_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new IdentityCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void EmptyMediator_ShouldThrowNullArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new IdentityCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public async Task Handle_GoodRequest_ShouldReturnTrue()
        {
            // Arrange
            var request = new IdentityEventProcessCommand
            {
                Data = TestDataProvider.GetTestData<string>("IdentityMS\\IdentityRequestWithChannelVRED")
            };
            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<bool>>(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ExceptionOccurs_ShouldReturnFalse()
        {

            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<IdentityRepoCommand>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new NullReferenceException());

            // Act
            var result = await _handler.Handle(new IdentityEventProcessCommand(), CancellationToken.None);

            // Assert
            Assert.IsType<Result<bool>>(result);
            Assert.False(result.Value);
        }

        [Fact]
        public async Task Handle_GoodRequest_ShouldReturnTrue1()
        {

            // Arrange
            var request = new IdentityEventProcessCommand
            {
                Data = TestDataProvider.GetTestData<string>("IdentityMS\\IdentityRequest")
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<bool>>(result);
            Assert.True(result.Value);
        }
    }
}
