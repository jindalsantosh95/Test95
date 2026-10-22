using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Command.AirportDetails;
using VAA.EAPI.Microservices.Retail.Order.Search.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class FlightInfoMessageSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;

        public FlightInfoMessageSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }
        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new FlightInfoMessageSubscriber(null));
        }
        [Fact]
        public async Task PaymentUpdateSubscriber_GoodRequestOrderCreate_ShouldSucceed()
        {
            // Arrange

            FlightInfoMessageSubscriber handler = new(_mediator.Object);
            FunctionContext functionContext = new TestFunctionContext();
            CloudEvent cloudEvent = new() { Id = "abcde-ffhfh-46575859-4657588t7", Source = "Order", Data = new AirportDetailsEventProcessCommand { } };

            // Act
            await handler.FlightInfoMessageSubscribe(Json.Serialize(cloudEvent), functionContext);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }

    }
}
