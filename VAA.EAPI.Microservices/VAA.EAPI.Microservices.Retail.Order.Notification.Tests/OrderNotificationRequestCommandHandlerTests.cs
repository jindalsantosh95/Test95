using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Tests
{
    public class OrderNotificationRequestCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderNotificationRequestCommandHandler>> _logger;
        private readonly OrderNotificationRequestCommandHandler _handler;
        public OrderNotificationRequestCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderNotificationRequestCommandHandler>>();
            _handler = new OrderNotificationRequestCommandHandler(_mediator.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            OrderNotificationTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationRequestCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderNotificationRequestCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(new OrderNotificationRequestCommand(), CancellationToken.None);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessStatusResponse()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessStatusResponse_WithoutMock()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessStatusResponse_Validation()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            orderNotificationEventProcessCommand.Channel = "xyz"; //Invalid channel
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnExceptionForInvalidOrderReference_Validation()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            orderNotificationEventProcessCommand.NotificationRequest.OrderReference = "";
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnExceptionForEmptyCreationDate_Validation()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            orderNotificationEventProcessCommand.NotificationRequest.CreationDate = "";
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnExceptionForInvalidCreationDateFormat_Validation()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            orderNotificationEventProcessCommand.NotificationRequest.CreationDate = "11/06/2022";
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnExceptionForInvalidCreationTimeFormat_Validation()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            orderNotificationEventProcessCommand.NotificationRequest.CreationTime = "11/06/2022T12:23";
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnExceptionForNullFulfillmentDetails_Validation()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            orderNotificationEventProcessCommand.NotificationRequest.FulfillmentDetails = null;
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnExceptionForInvalidFulfillmentDetails_Validation()
        {
            var orderNotificationEventProcessCommand = TestDataProvider.GetTestData<OrderNotificationRequestCommand>("OrderNotifyRequestCommand");
            orderNotificationEventProcessCommand.NotificationRequest.FulfillmentDetails.First().FulfillmentId = "";
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderNotificationEventProcessCommand, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }
    }
}
