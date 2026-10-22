using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.PaymentUpdate;
using Xunit;
namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class PaymentUpdateRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly PaymentUpdateRepoCommandHandler _handler;
        public PaymentUpdateRepoCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _handler = new PaymentUpdateRepoCommandHandler(_mediator.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PaymentUpdateRepoCommandHandler(null));
        }
        [Fact]
        public async Task PaymentUpdateRepoCommandHandler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _handler.Handle(null, CancellationToken.None));
        }
    }
}
