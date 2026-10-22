using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicketOrder;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Tests
{
    public class VoidOrderDetailEventProcessCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<VoidOrderDetailEventProcessCommandHandler>> _logger;
        private readonly VoidOrderDetailEventProcessCommandHandler _handler;
        public VoidOrderDetailEventProcessCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<VoidOrderDetailEventProcessCommandHandler>>();
            _handler = new VoidOrderDetailEventProcessCommandHandler(_mediator.Object, _logger.Object);
            OrderTicketTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new VoidOrderDetailEventProcessCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VoidOrderDetailEventProcessCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task VoidOrderDetailEventProcessCommandHandler_ShouldReturnFailure_WhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _handler.Handle(new VoidOrderDetailEventProcessCommand(), CancellationToken.None));
        }

        [Fact]
        public void isOrderStatusRFC_ShouldSucceed_WithGoodRequest()
        {
            var voidOrderDetails = TestDataProvider.GetTestData<VoidOrderDetails>("TicketMS\\VoidTicketDetails");
            var result = _handler.isOrderStatusRFC(voidOrderDetails);
            Assert.True(result);
        }

        [Fact]
        public void isOrderStatusRFC_ShouldFail_WithGoodRequest()
        {
            var voidOrderDetails = TestDataProvider.GetTestData<VoidOrderDetails>("TicketMS\\VoidTicketDetails");
            voidOrderDetails.OrderData.OrderStatus = "Ordered";
            var result = _handler.isOrderStatusRFC(voidOrderDetails);
            Assert.False(result);
        }

        [Fact]
        public void ValidateRiskFreeCancellation_ShouoldFail_WithInvalidDate()
        {
            var voidOrderDetails = TestDataProvider.GetTestData<VoidOrderDetails>("TicketMS\\VoidTicketDetails");
            var result = _handler.ValidateRiskFreeCancellation(voidOrderDetails);
            Assert.False(result);
        }

        [Fact]
        public void ValidateRiskFreeCancellation_ShouldSucceed_WithGoodRequest()
        {
            var voidOrderDetails = TestDataProvider.GetTestData<VoidOrderDetails>("TicketMS\\VoidTicketDetails");
            voidOrderDetails.Tickets.First().LastTransactionEventGTs = DateTime.UtcNow.AddHours(-1).ToString("yyyyMMddhhmmss");
            ConfigReader.Set(Constants.Environment.RefundWindowInHours, "24");
            var result = _handler.ValidateRiskFreeCancellation(voidOrderDetails);
            Assert.True(result);
        }
        [Fact]
        public async Task VoidOrderDetailEventProcessCommandHandler_ShouldSucceed_WithOrderStatusNonRFC()
        {
            var voidOrderDetail = TestDataProvider.GetTestData<VoidOrderDetailEventProcessCommand>("TicketMS\\VoidOrderDetails-RFC");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(voidOrderDetail, CancellationToken.None);
            Assert.True(Unit.Value == result.Value);
        }

        [Fact]
        public async Task VoidOrderDetailEventProcessCommandHandler_ShouldSuccees_WithGoodRequest()
        {
            var voidOrderDetailEventProcessCommand = TestDataProvider.GetTestData<VoidOrderDetailEventProcessCommand>("TicketMS\\VoidOrderDetails-RFC");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(voidOrderDetailEventProcessCommand, CancellationToken.None);
            Assert.True(Unit.Value == result.Value);
        }

        [Fact]
        public async Task VoidOrderDetailEventProcessCommandHandler_ShouldReturnUnitValueSuccess_WithChangedOrderStatus()
        {
            var voidOrderDetailEventProcessCommand = TestDataProvider.GetTestData<VoidOrderDetailEventProcessCommand>("TicketMS\\VoidOrderDetails-RFC-OK");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(voidOrderDetailEventProcessCommand, CancellationToken.None);
            Assert.True(Unit.Value == result.Value);

        }
    }
}

