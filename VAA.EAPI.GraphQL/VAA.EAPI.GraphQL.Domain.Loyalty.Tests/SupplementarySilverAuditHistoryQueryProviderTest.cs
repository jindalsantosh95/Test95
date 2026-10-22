using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class SupplementarySilverAuditHistoryQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public SupplementarySilverAuditHistoryQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new SupplementarySilverAuditHistoryQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new SupplementarySilverAuditHistoryQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetSupplementarySilverAuditHistoryShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<APIResponse<SupplementarySilverAuditHistoryResponse>>("Audit\\SupplementarySilverAuditHistoryAPIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");
            _mockConnector.Setup(m => m.GetAsync<SupplementarySilverAuditHistoryResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new SupplementarySilverAuditHistoryQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);

            var queryResponse = await provider.GetSupplementarySilverAuditHistory("XYZ");

            Assert.IsType<SupplementarySilverAuditHistoryResponse>(queryResponse);
            Assert.NotNull(response.Response);
        }
    }
}