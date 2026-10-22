using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class ContactHistoryQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public ContactHistoryQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactHistoryQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new ContactHistoryQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetContactHistoryShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<APIResponse<ContactHistoryAuditResponse>>("Audit\\ContactHistoryAPIResponse");
            var customerResponse = TestDataProvider.GetTestData<APIResponse<Customers>>("Audit\\GetCustomerByCustId");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<ContactHistoryAuditResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);
            _mockConnector.Setup(m => m.GetAsync<Customers>(It.IsAny<string>(),
                                                                    It.IsAny<Dictionary<string, string>>(),
                                                                    It.IsAny<string>())).ReturnsAsync(customerResponse);

            var provider = new ContactHistoryQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.GetContactHistory("XYZ");


            Assert.IsType<ContactHistoryAuditResponse>(queryResponse);
            Assert.NotNull(response.Response);
        }
    }
}
