using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Application.Command.PhoneUpdate;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Tests;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeed.Tests
{
    public class CustomerSearchUpdateCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public CustomerSearchUpdateCommandHandlerTests()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchUpdateCommandHandler(null));
        }       
    }
}