using MediatR;
using Moq;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.SupplementarySilver;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.Award
{
    public class SupplementarySilverAuditHistoryRepoQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public SupplementarySilverAuditHistoryRepoQueryHandlerTest()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SupplementarySilverAuditHistoryRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(new List<object>());
            SupplementarySilverAuditHistoryRepoQuery query = (SupplementarySilverAuditHistoryRepoQuery)"CustomerId";
            SupplementarySilverAuditHistoryRepoQueryHandler handler = new SupplementarySilverAuditHistoryRepoQueryHandler(_mockMediator.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}