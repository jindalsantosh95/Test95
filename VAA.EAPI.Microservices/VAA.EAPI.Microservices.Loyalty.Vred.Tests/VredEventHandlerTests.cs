using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Threading.Tasks;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;
using VAA.EAPI.Microservices.Loyalty.Vred.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class VredEventHandlerTests
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
            string input = "{\"id\":\"123\",\"type\":\"\",\"subject\":\"\",\"time\":\"2021-12-13T12:53:13.4770788Z\",\"data\":{\"firstName\":\"pologer\"},\"dataVersion\":\"1.0\"}";

            // Act
            await handler.Run(input, functionContext);

            // Assert
            // The Event Grid Handler does not have any outcome. 
        }

    }
}
