using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentStatusEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderPaymentStatusEventCommandHandler>> _logger;
        private readonly OrderPaymentStatusEventCommandHandler _handler;


        public OrderPaymentStatusEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderPaymentStatusEventCommandHandler>>();
            _handler = new OrderPaymentStatusEventCommandHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderPaymentStatusEventCommandHandler(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderPaymentStatusEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            var request = TestDataProvider.GetTestData<OrderPaymentStatusEventCommand>("PaymentMS\\OrderPaymentStatusEventCommand");

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentNullException());

            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(new OrderPaymentStatusEventCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccess()
        {
            var request = TestDataProvider.GetTestData<OrderPaymentStatusEventCommand>("PaymentMS\\OrderPaymentStatusEventCommand");

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }
    }
}
