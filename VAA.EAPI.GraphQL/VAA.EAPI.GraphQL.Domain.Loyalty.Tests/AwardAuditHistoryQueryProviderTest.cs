using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class AwardAuditHistoryQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public AwardAuditHistoryQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardAuditHistoryQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new AwardAuditHistoryQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetAwardAuditHistoryShouldSucceed()
        {
            var awardAuditHistoryApiResponse = TestDataProvider.GetTestData<APIResponse<AwardAuditHistoryResponse>>("Audit\\AwardAuditHistoryAPIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");
            _mockConnector.Setup(m => m.GetAsync<AwardAuditHistoryResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(awardAuditHistoryApiResponse);
            var provider = new AwardAuditHistoryQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);

            var queryResponse = await provider.GetAwardAuditHistory("XYZ");

            Assert.IsType<AwardAuditHistoryResponse>(queryResponse);
            Assert.NotNull(awardAuditHistoryApiResponse.Response);
        }
    }
}