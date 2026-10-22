using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Threading.Tasks;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class AddressLookupSubscriberTests
    {
        [Fact]
        public async Task Run_GoodRequest_ShouldSucceed()
        {
            // Arrange
            Mock<IMediator> mockMediator = new Mock<IMediator>();
            AddressLookupMessageSubscriber handler = new AddressLookupMessageSubscriber(mockMediator.Object);
            FunctionContext functionContext = new TestFunctionContext();
            string cloudEvent = "{\"type\":\"CustomerEnrolmentInitiated\",\"source\":\"enrolcustomer\",\"subject\":\"\",\"id\":\"123\",\"time\":\"2022-02-09T14:26:13.4770788Z\",\"datacontenttype\":\"application/json\",\"data\":{\"ChannelUserId\":\"VRED\",\"FirstName\":\"groovereser\"}}";
            await handler.Run(cloudEvent, functionContext);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }
    }
}
