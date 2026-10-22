using MediatR;
using Moq;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Award;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.Award
{
    public class AwardAuditHistoryRepoQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public AwardAuditHistoryRepoQueryHandlerTest()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardAuditHistoryRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(new List<object>());
            AwardAuditHistoryRepoQuery query = new AwardAuditHistoryRepoQuery("CustomerId");
            AwardAuditHistoryRepoQueryHandler handler = new AwardAuditHistoryRepoQueryHandler(_mockMediator.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}