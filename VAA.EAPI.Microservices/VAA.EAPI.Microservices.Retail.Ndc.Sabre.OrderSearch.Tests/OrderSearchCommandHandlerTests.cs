using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Tests
{
    public class OrderSearchCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderSearchCommandHandler>> _logger;
        private readonly OrderSearchCommandHandler _handler;

        public OrderSearchCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderSearchCommandHandler>>();
            _handler = new OrderSearchCommandHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderSearchCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderSearchCommandHandler(_mediator.Object, null));
        }

    }
}
