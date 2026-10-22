using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Tests
{
    public class OrderCreateCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderCreateCommandHandler>> _logger;
        private readonly OrderCreateCommandHandler _command;

        public OrderCreateCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderCreateCommandHandler>>();
            _command = new OrderCreateCommandHandler(_mediator.Object, _logger.Object);
            ConfigReader.Set("ServiceBusTopicName", "test");
            ConfigReader.Set("ServiceBusConnectionString", "dummyConnString");
            ConfigReader.Set("OrderCreateResponseUrl", "http://localhost:7071/order/response/{correlationid}");
        }

        [Fact]
        public void HandlerShouldThrowException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void HandlerShouldThrowException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task HandlerShouldThrowException_WhenServiceBusThrowsException()
        {
            //Arrange
            var request = new OrderCreateCommand(new Domain.OrderCreateRequest());
            _mediator.Setup(x => x.Send(It.IsAny<ServiceBusTopicMessageEventCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            //Act
            var result = await _command.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("G009", result.Error);
        }

        [Fact]
        public async Task HandlerShouldThrowException_WithNullRequest()
        {
            //Arrange
            var request = new OrderCreateCommand(null);

            //Act
            var result = await _command.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("G009", result.Error);
        }

        [Fact]
        public async Task HandlerShouldCompleteSuccessfully_WithGoodRequest()
        {
            //Arrange
            var request = new OrderCreateCommand(new Domain.OrderCreateRequest());

            //Act
            var result = await _command.Handle(request, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
        }
    }
}
