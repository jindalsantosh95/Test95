using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.RefundUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class RefundUpdateSubscriberTests
    {
        private readonly Mock<IMediator> _mediator;

        public RefundUpdateSubscriberTests()
        {
            _mediator = new Mock<IMediator>();

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RefundUpdateSubscriber(null));
        }

        [Fact]
        public async Task RefundUpdateSubscriber_GoodRequestOrderCreate_ShouldSucceed()
        {
            // Arrange

            RefundUpdateSubscriber handler = new(_mediator.Object);
            FunctionContext functionContext = new TestFunctionContext();
            CloudEvent cloudEvent = new() { Id = "abcde-ffhfh-46575859-4657588t7", Source = "Payment", Data = new RefundUpdateCommand { } };

            // Act
            await handler.Run(Json.Serialize(cloudEvent), functionContext);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }
    }
}
