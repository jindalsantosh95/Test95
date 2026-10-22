using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class OrderCancelRequestCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderCancelRequestCommandHandler>> _logger;
        private readonly OrderCancelRequestCommandHandler _command;

        public OrderCancelRequestCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderCancelRequestCommandHandler>>();
            _command = new OrderCancelRequestCommandHandler(_mediator.Object, _logger.Object);
            ConfigReader.Set("ServiceBusConnectionString", "dummyConnString");
            ConfigReader.Set("NDCCancelPnrTopicName", "test");
        }

        [Fact]
        public void HandlerShouldThrowException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCancelRequestCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void HandlerShouldThrowException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCancelRequestCommandHandler(_mediator.Object, null));
        }

    }

}


