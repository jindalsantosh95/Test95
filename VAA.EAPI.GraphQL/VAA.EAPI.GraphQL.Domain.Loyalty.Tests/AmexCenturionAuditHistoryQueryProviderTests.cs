using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class AmexCenturionAuditHistoryQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public AmexCenturionAuditHistoryQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new AmexCenturionAuditHistoryQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new AmexCenturionAuditHistoryQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetAmexCenturionAuditHistoryShouldSucceed()
        {
            var mockApiResponse = TestDataProvider.GetTestData<APIResponse<AmexCenturionAuditHistoryResponse>>("Audit\\AmexCenturionAuditHistoryAPIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");
            _mockConnector.Setup(m => m.GetAsync<AmexCenturionAuditHistoryResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(mockApiResponse);
            var provider = new AmexCenturionAuditHistoryQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);

            var queryResponse = await provider.GetAmexCenturionAuditHistory("XYZ");

            Assert.IsType<AmexCenturionAuditHistoryResponse>(queryResponse);
            Assert.NotNull(mockApiResponse.Response);
            Assert.Equal(mockApiResponse.Response.AmexCenturionAuditHistory.First().CardLinkingDocID, queryResponse.AmexCenturionAuditHistory.First().CardLinkingDocID);
            Assert.Equal(mockApiResponse.Response.AmexCenturionAuditHistory.First().PartnerCode, queryResponse.AmexCenturionAuditHistory.First().PartnerCode);
            Assert.Equal(mockApiResponse.Response.AmexCenturionAuditHistory.First().StatusEnhancementCode, queryResponse.AmexCenturionAuditHistory.First().StatusEnhancementCode);
        }
    }
}
