using MediatR;
using Moq;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories.Preference;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests.Preference
{
    public class PreferenceHistoryRepoQueryHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator = new();

        public PreferenceHistoryRepoQueryHandlerTest()
        {
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PreferenceAuditHistoryRepoQueryHandler(null));
        }

        [Fact]
        public async Task HandleShouldSucceedWithoutError()
        {
            // Arrange
            _mockMediator.Setup(s => s.Send(It.IsAny<CosmosReaderServiceQuery>(), CancellationToken.None)).ReturnsAsync(new List<object>());
            PreferenceAuditHistoryRepoQuery query = new PreferenceAuditHistoryRepoQuery("CustomerId");
            PreferenceAuditHistoryRepoQueryHandler handler = new PreferenceAuditHistoryRepoQueryHandler(_mockMediator.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }
}