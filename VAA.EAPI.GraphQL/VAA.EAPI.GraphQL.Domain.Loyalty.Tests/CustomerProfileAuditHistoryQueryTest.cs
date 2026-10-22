using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ProfileAudit;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CustomerProfileAuditHistoryQueryTest
    {
        private readonly Mock<ICustomerProvider> _customerService;
        private readonly Mock<ICustomerProfileAuditHistoryQueryProvider> _customerProfileAuditHistoryQueryProvider;
        private readonly Mock<ILogger<CustomerProfileAuditHistoryQuery>> _logger;
        private readonly CustomerProfileAuditHistoryQuery _customerProfileAuditHistoryQuery;

        public CustomerProfileAuditHistoryQueryTest()
        {
            _customerService = new Mock<ICustomerProvider>();
            _customerProfileAuditHistoryQueryProvider = new Mock<ICustomerProfileAuditHistoryQueryProvider>();
            _logger = new Mock<ILogger<CustomerProfileAuditHistoryQuery>>();
            _customerProfileAuditHistoryQuery = new CustomerProfileAuditHistoryQuery(_logger.Object, _customerService.Object, _customerProfileAuditHistoryQueryProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditHistoryQuery(null, _customerService.Object, _customerProfileAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditHistoryQuery(_logger.Object, null, _customerProfileAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileAuditHistoryQuery(_logger.Object, _customerService.Object, null));
        }

        [Fact]
        public async Task CustomerProfileAuditHistoryQueryShouldSucceedWithCustomerID()
        {
            var response = TestDataProvider.GetTestData<CustomerProfileAuditHistoryResponse>("Audit\\CustomerProfileAuditHistoryResponse");
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _customerProfileAuditHistoryQueryProvider.Setup(m => m.GetCustomerProfileAuditHistory(customerId)).ReturnsAsync(response);
            var queryResponse = await _customerProfileAuditHistoryQuery.RetrieveCustomerProfileAuditHistory(customerId, null);

            Assert.IsType<CustomerProfileAuditHistoryResponse>(queryResponse);
            Assert.True(response.CustomerProfileHistoryAudit.Count() == queryResponse.CustomerProfileHistoryAudit.Count());
        }

        [Fact]
        public async Task CustomerProfileAuditHistoryQueryShouldSucceedWithMemberNumber()
        {
            var response = TestDataProvider.GetTestData<CustomerProfileAuditHistoryResponse>("Audit\\CustomerProfileAuditHistoryResponse");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            customerDTO.Customer.CustomerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _customerService.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _customerProfileAuditHistoryQueryProvider.Setup(m => m.GetCustomerProfileAuditHistory("69b8f74b-e412-4a31-96e0-352b9603dfd3")).ReturnsAsync(response);
            var queryResponse = await _customerProfileAuditHistoryQuery.RetrieveCustomerProfileAuditHistory(null, "9106488951");

            Assert.IsType<CustomerProfileAuditHistoryResponse>(queryResponse);
            Assert.True(response.CustomerProfileHistoryAudit.Count() == queryResponse.CustomerProfileHistoryAudit.Count());
        }

        [Fact]
        public async Task CustomerProfileAuditHistoryQueryShouldThrowException()
        {
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _customerProfileAuditHistoryQueryProvider.Setup(m => m.GetCustomerProfileAuditHistory(customerId)).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _customerProfileAuditHistoryQuery.RetrieveCustomerProfileAuditHistory(customerId, null));
        }

        [Fact]
        public async Task CustomerProfileAuditHistoryQueryShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _customerProfileAuditHistoryQuery.RetrieveCustomerProfileAuditHistory(null, null));
        }
    }
}