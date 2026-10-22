using MediatR;
using Moq;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.AmexCenturion;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.AmexCenturion
{
    public class AmexCenturionAuditHistoryRepoQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public AmexCenturionAuditHistoryRepoQueryHandlerTests()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AmexCenturionAuditHistoryRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            var response = TestDataProvider.GetTestData<List<object>>("AuditMS\\AmexCenturion\\AmexCenturionAuditHistory");
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(response);
            AmexCenturionAuditHistoryRepoQuery query = new AmexCenturionAuditHistoryRepoQuery("CustomerId");
            AmexCenturionAuditHistoryRepoQueryHandler handler = new AmexCenturionAuditHistoryRepoQueryHandler(_mockMediator.Object);
          
            // Act
            var result = await handler.Handle(query, CancellationToken.None);
          
            // Assert
            Assert.NotNull(result);          
            Assert.Equal("AMXIC1", result.First().StatusEnhancementCode);
            Assert.Equal("D92252A8-5198-6F04-E053-7D41D70A1575", result.First().CustomerId);
        }
    }
}
