using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ProfileAudit;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CustomerProfileAuditHistoryQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public CustomerProfileAuditHistoryQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditHistoryQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditHistoryQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetCustomerProfileAuditHistoryShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<APIResponse<CustomerProfileAuditHistoryResponse>>("Audit\\CustomerProfileAuditHistoryAPIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");

            _mockConnector.Setup(m => m.GetAsync<CustomerProfileAuditHistoryResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new CustomerProfileAuditHistoryQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);

            var queryResponse = await provider.GetCustomerProfileAuditHistory("XYZ");

            Assert.IsType<CustomerProfileAuditHistoryResponse>(queryResponse);
            Assert.NotNull(queryResponse.CustomerProfileHistoryAudit);
        }
    }
}