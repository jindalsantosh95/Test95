using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.RefundUpdate;
using Xunit;
namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class RefundUpdateRepoHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly RefundUpdateRepoHandler _handler;
        public RefundUpdateRepoHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _handler = new RefundUpdateRepoHandler(_mediator.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RefundUpdateRepoHandler(null));
        }
        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _handler.Handle(null, CancellationToken.None));
        }
    }
}
