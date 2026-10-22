using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CustomerQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public CustomerQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new CustomerQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetCustomerSearchShouldSucceed()
        {
            var customerSearch = TestDataProvider.GetTestData<APIResponse<IEnumerable<CustomerSearch>>>("Loyalty\\CustomerSearchDoc");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<IEnumerable<CustomerSearch>>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(customerSearch);

            var provider = new CustomerQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);

            var queryResponse = await provider.GetCustomerSearch("D47166A3-533D-9159-E053-6941D90A1989");

            Assert.NotNull(queryResponse);
            Assert.Equal("D47166A3-533D-9159-E053-6941D90A1989", queryResponse.First().CustomerId);
            Assert.Equal("test222555@gmail.com", queryResponse.First().EmailAddress);
            Assert.Equal("9876543210", queryResponse.First().PhoneNumber);
        }
    }
}
