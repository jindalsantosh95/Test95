using MediatR;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class OrderMessageSubscriberTests
    {
        private readonly Mock<IMediator> _mediator;

        public OrderMessageSubscriberTests()
        {
            _mediator = new Mock<IMediator>();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderMessageSubscriber(null));
        }

        [Fact]
        public async Task OrderMessageSubscriber_GoodRequestOrderCreate_ShouldSucceed()
        {
            // Arrange

            OrderMessageSubscriber handler = new(_mediator.Object);
            CloudEvent cloudEvent = new() { Id = "abcde-ffhfh-46575859-4657588t7", Source = "Order", Data = new OrderCreateRequest { } };

            // Act
            await handler.Run(Json.Serialize(cloudEvent));

            // Assert
            // The service bus event Handler does not have any outcome. 
        }
    }
}
