using MediatR;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Triggers.ServiceBus;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Tests
{
    public class OrderCreateMessageSubscriberTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly OrderCreateMessageSubscriber _controller;

        public OrderCreateMessageSubscriberTests()
        {
            _mediator = new Mock<IMediator>();
            _controller = new OrderCreateMessageSubscriber(_mediator.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void ControllerShouldThrowException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderCreateMessageSubscriber(null));
        }

        [Fact]
        public async Task ControllerShouldRunSuccessfully_WithGoodRequest()
        {
            //Arrange
            var request = new OrderCreateEventProcessCommand();

            //Act
           await _controller.Run(Json.Serialize(request));

            //Assert
            //Should not throw any exception
        }
    }
}
