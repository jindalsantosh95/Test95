using MediatR;
using Moq;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Tests
{
    public class CustomerSearchUpdateRepoCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public CustomerSearchUpdateRepoCommandHandlerTests()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerSearchUpdateRepoCommandHandler(null));
        }
    }
}
