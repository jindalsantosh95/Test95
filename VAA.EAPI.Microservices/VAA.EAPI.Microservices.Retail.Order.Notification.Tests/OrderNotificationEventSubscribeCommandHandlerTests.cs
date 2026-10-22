using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Tests
{
    public class OrderNotificationEventSubscribeCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderNotificationEventSubscribeCommandHandler>> _logger;
        private readonly OrderNotificationEventSubscribeCommandHandler _handler;
        public OrderNotificationEventSubscribeCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderNotificationEventSubscribeCommandHandler>>();
            _handler = new OrderNotificationEventSubscribeCommandHandler(_mediator.Object, _logger.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationEventSubscribeCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationEventSubscribeCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _handler.Handle(new OrderNotificationEventProcessCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessStatusResponse()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationEventProcessCommand>("OrderNotifyEventCommand");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(Unit.Value == result.Value);
        }
    }
}
