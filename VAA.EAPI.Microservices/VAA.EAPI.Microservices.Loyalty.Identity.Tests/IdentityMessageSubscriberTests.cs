using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Threading.Tasks;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.ServiceBus;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    public class IdentityMessageSubscriberTests
    {
        [Fact]
        public async Task Run_GoodRequest_ShouldSucceed()
        {
            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<CreateCustomerEntities>(), new System.Threading.CancellationToken()))
            .ReturnsAsync(true);
            IdentityMessageSubscriber handler = new IdentityMessageSubscriber(mockMediator.Object);
            FunctionContext functionContext = new TestFunctionContext();
            string input = TestDataProvider.GetTestData<string>("IdentityMS\\IdentityMessageSubscriberRequest");

            // Act
            await handler.Run(input, functionContext);

            // Assert
            // The Event Grid Handler does not have any outcome. 
        }
    }
}
