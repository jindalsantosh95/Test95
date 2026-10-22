using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCreate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class OrderCreateCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<OrderCreateRequestCommandHandler>> _mockLogger;
        private readonly OrderCreateRequestCommandHandler _mockHandler;

        public OrderCreateCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<OrderCreateRequestCommandHandler>>();
            _mockHandler = new OrderCreateRequestCommandHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderCreateRequestCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderCreateRequestCommandHandler(_mockMediator.Object, null));
        }

        [Fact]
        public async Task OrderCreateCommandHandler_ShouldReturnFailure_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _mockHandler.Handle(new OrderCreateRequestCommand(), CancellationToken.None));
        }
        [Fact]
        public async Task OrderCreateCommandHandler_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                        .Equals(true);

            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _mockHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task OrderCreateCommandHandler_ShouldReturnAcceptedStatus_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<OrderCreateRequestCommand>("OrderData.OrderCreateCommand");

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }
    }
}
