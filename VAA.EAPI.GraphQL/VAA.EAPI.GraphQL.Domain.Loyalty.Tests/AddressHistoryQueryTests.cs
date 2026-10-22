using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class AddressHistoryQueryTests
    {
        private readonly Mock<ICustomerProvider> _customerService;
        private readonly Mock<IAddressHistoryQueryProvider> _addressHistoryQueryProvider;
        private readonly AddressHistoryQuery _addressHistoryQuery;

        public AddressHistoryQueryTests()
        {
            _customerService = new Mock<ICustomerProvider>();
            _addressHistoryQueryProvider = new Mock<IAddressHistoryQueryProvider>();
            _addressHistoryQuery = new AddressHistoryQuery(_customerService.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AddressHistoryQuery(null));
        }

        [Fact]
        public async Task AddressHistoryQuery_ThrowException_NullReferences()
        {
            // Null AddressHistoryQueryProvider
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addressHistoryQuery.RetrieveAddressHistory("xxx", "xxx", null));
            // No cust ID or Memb ID
            await Assert.ThrowsAsync<ArgumentException>(() => _addressHistoryQuery.RetrieveAddressHistory("", "", _addressHistoryQueryProvider.Object));
        }

        [Fact]
        public async Task AddressHistoryQueryShouldReturnSucceedWithGoodResponse()
        {
            var response = TestDataProvider.GetTestData<IEnumerable<AddressHistory>>("Audit\\AddressHistory");
            var customerId = "D4718748-3560-E0ED-E053-6941D90AAD4B";
            _addressHistoryQueryProvider.Setup(m => m.GetAddressHistory(customerId)).ReturnsAsync(response);
            var queryResponse = await _addressHistoryQuery.RetrieveAddressHistory(customerId, null, _addressHistoryQueryProvider.Object);
            Assert.IsType<List<AddressHistory>>(queryResponse);
        }

        [Fact]
        public async Task AddressHistoryQueryShouldReturnSucceedWithMemberNumber()
        {
            var response = TestDataProvider.GetTestData<IEnumerable<AddressHistory>>("Audit\\AddressHistory");
            var memberNumer = "1234567890";
            var cust = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            _customerService.Setup(c => c.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cust);
            _addressHistoryQueryProvider.Setup(m => m.GetAddressHistory(cust.Customer.CustomerId)).ReturnsAsync(response);
            var queryResponse = await _addressHistoryQuery.RetrieveAddressHistory(null, memberNumer, _addressHistoryQueryProvider.Object);
            Assert.IsType<List<AddressHistory>>(queryResponse);
        }
    }
}
