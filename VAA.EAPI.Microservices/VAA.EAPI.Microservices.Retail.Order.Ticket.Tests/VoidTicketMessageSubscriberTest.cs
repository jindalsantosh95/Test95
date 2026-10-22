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
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Tests
{
    public class VoidTicketMessageSubscriberTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<VoidTicketMessageSubscriber>> _logger;

        public VoidTicketMessageSubscriberTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<VoidTicketMessageSubscriber>>();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task VoidTicketMessageSubscriber_ShouldSucceed_WithGoodRequest()
        {

            VoidTicketMessageSubscriber handler = new VoidTicketMessageSubscriber(_mediator.Object, _logger.Object);
            FunctionContext functionContext = new TestFunctionContext();
            CloudEvent cloudEvent = new() { Id = "abcde-ffhfh-46575859-4657588t7", Source = "Order", Data = new VoidTicketEventCommand { } };

            // Act
            await handler.Run(Json.Serialize(cloudEvent), functionContext);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VoidTicketMessageSubscriber(_mediator.Object, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new VoidTicketMessageSubscriber(null, _logger.Object));
        }
    }
}
