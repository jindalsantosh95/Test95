using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Notification.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Tests
{
    public class OrderNotificationSubscriberTests
    {
        public OrderNotificationSubscriberTests()
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public async Task Run_GoodRequest_ShouldSucceed()
        {
            // Arrange

            Mock<IMediator> mockMediator = new Mock<IMediator>();
            Mock<ILogger<OrderNotificationMessageSubscriber>> mockLogger = new Mock<ILogger<OrderNotificationMessageSubscriber>>();
            OrderNotificationMessageSubscriber handler = new OrderNotificationMessageSubscriber(mockMediator.Object, mockLogger.Object);
            CloudEvent cloudEvent = new() { Id = "abcde-ffhfh-46575859-4657588t7", Source = "Order", Data = new OrderNotificationEventProcessCommand { } };

            // Act
            await handler.Run(Json.Serialize(cloudEvent), AsyncContext.Current.Context);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }
    }
}
