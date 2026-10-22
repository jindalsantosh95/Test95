using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.OrderPayment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class OrderPaymentEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderPaymentEventCommandHandler>> _logger;
        private readonly OrderPaymentEventCommandHandler _handler;


        public OrderPaymentEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderPaymentEventCommandHandler>>();
            _handler = new OrderPaymentEventCommandHandler(_mediator.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            OrderPaymentTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderPaymentEventCommandHandler(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderPaymentEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(new OrderPaymentEventCommand(), CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedStatusResponse()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessStatusResponse_Mock()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            ConfigReader.Set("MockResponseEnabled", "Y");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnSuccessStatusResponse_WithoutMock()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            ConfigReader.Set("MockResponseEnabled", "N");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidChannel()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.Channel = "Web-DF1";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidOrderReference()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.OrderReference = "";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }


        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidTransactionId()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().TransactionId = 0;

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidTransactionAmount()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().TransactionAmount = -1;

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }


        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidTransactionDate()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().TransactionDateTime = "2022-04-29";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnNoBusinessExceptionForPaymentMethodNotSelectedAsCard()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().PaymentMethod = PaymentMethod.DD;

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnNoBusinessExceptionForPaymentTypeNotSelectedAsCard()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().PaymentType = PaymentType.Virtual;

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidPaymentSubTypeForPaymentMethodSelectedAsCard()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().PaymentSubType = null;

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidCardExpiryForPaymentMethodSelectedAsCard()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().CardExpiry = "12-222";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidCardNameForPaymentMethodSelectedAsCard()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().CardName = "";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidCardNumberForPaymentMethodSelectedAsCard()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().CardNumber = "";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidCurrency()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().Currency = "";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidPspName()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().PspName = "";

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task Handler_ShouldReturnBusinessExceptionForInvalidPspId()
        {
            var orderPaymentRequest = TestDataProvider.GetTestData<OrderPaymentEventCommand>("PaymentMS\\OrderPaymentRequest");
            orderPaymentRequest.PaymentRequest.First().PspId = 0;

            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestInsertCommandHandler>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>()))
             .Equals(true);

            var result = await _handler.Handle(orderPaymentRequest, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Status == Status.BadRequest);
        }
    }
}
