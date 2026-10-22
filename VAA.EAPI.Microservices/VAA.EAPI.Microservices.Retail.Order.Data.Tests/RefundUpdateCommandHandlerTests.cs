using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.RefundUpdate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class RefundUpdateCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<RefundUpdateCommandHandler>> _logger;
        private readonly RefundUpdateCommandHandler _handler;


        public RefundUpdateCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<RefundUpdateCommandHandler>>();
            _handler = new RefundUpdateCommandHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RefundUpdateCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RefundUpdateCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task RefundUpdateCommandHandler_ShouldThrowNullException_WhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(new RefundUpdateCommand(), CancellationToken.None));
        }
        [Fact]
        public async Task RefundUpdateCommandHandler_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                        .Equals(true);

            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task RefundUpdateCommandHandler_ShouldReturnAcceptedStatus_WithGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<RefundUpdateCommand>("PaymentMS\\OrderPaymentCommandRequest");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }
    }
}
