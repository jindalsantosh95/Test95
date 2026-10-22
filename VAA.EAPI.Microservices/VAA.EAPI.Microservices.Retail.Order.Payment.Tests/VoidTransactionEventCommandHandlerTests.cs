using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.VoidTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class VoidTransactionEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<VoidTransactionEventCommandHandler>> _logger;
        private readonly VoidTransactionEventCommandHandler _handler;
        public VoidTransactionEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<VoidTransactionEventCommandHandler>>();
            _handler = new VoidTransactionEventCommandHandler(_mediator.Object, _logger.Object);
            OrderPaymentTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VoidTransactionEventCommandHandler(null, _logger.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VoidTransactionEventCommandHandler(_mediator.Object, null));
        }
        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _handler.Handle(new VoidTransactionEventCommand(), CancellationToken.None));
        }
        [Fact]
        public async Task Handler_CPDEvent_ShouldReturnSuccessStatusResponse()
        {
            var voidTransactionEventCommand = TestDataProvider.GetTestData<VoidTransactionEventCommand>("PaymentMS\\VoidOrderDetails-CPD");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<RefundAPIRepoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);
            var result = await _handler.Handle(voidTransactionEventCommand, CancellationToken.None);
            Assert.True(Unit.Value == result.Value);
        }
        [Fact]
        public async Task Handler_EmailEvent_ShouldReturnSuccessStatusResponse()
        {
            var voidTransactionEventCommand = TestDataProvider.GetTestData<VoidTransactionEventCommand>("PaymentMS\\VoidOrderDetails-CPD");
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<RefundAPIRepoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);
            var result = await _handler.Handle(voidTransactionEventCommand, CancellationToken.None);
            Assert.True(Unit.Value == result.Value);
        }
    }
}
