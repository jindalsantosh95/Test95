using MediatR;
using Moq;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.CustomerProfile
{
    public class CustomerProfileAuditHistoryRepoQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public CustomerProfileAuditHistoryRepoQueryHandlerTest()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditHistoryRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(new List<object>());
            CustomerProfileAuditHistoryRepoQuery query = new CustomerProfileAuditHistoryRepoQuery("CustomerId");
            CustomerProfileAuditHistoryRepoQueryHandler handler = new CustomerProfileAuditHistoryRepoQueryHandler(_mockMediator.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}