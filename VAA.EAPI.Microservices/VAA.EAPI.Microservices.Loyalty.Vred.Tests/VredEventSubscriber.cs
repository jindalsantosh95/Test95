using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;
using VAA.EAPI.Microservices.Loyalty.Vred.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class VredEventSubscriber
    {
        [Fact]
        public async Task Run_GoodRequest_ShouldSucceed()
        {
            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<CreateCustomerEntities>(), new System.Threading.CancellationToken()))
            .ReturnsAsync(true);
            VredCustomerSyncMessageSubscriber handler = new VredCustomerSyncMessageSubscriber(mockMediator.Object);
            FunctionContext functionContext = new TestFunctionContext();
            CloudEvent cloudEvent = new() { Id = "23332-QERR44-2332323-2323232332", Source = "VredCustomerSyncMessageSubscriber", Data = new CreateCustomerEntities { } };            // Act
            await handler.Run(Json.Serialize(cloudEvent), functionContext);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }

    }
}
