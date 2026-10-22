using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PreferenceAuditHistoryQueryTest
    {
        private readonly Mock<ICustomerProvider> _customerService;
        private readonly Mock<IPreferenceAuditHistoryQueryProvider> _preferenceAuditHistoryQueryProvider;
        private readonly Mock<ILogger<PreferenceAuditHistoryQuery>> _logger;
        private readonly PreferenceAuditHistoryQuery _preferenceAuditHistoryQuery;

        public PreferenceAuditHistoryQueryTest()
        {
            _customerService = new Mock<ICustomerProvider>();
            _preferenceAuditHistoryQueryProvider = new Mock<IPreferenceAuditHistoryQueryProvider>();
            _logger = new Mock<ILogger<PreferenceAuditHistoryQuery>>();
            _preferenceAuditHistoryQuery = new PreferenceAuditHistoryQuery(_logger.Object, _customerService.Object, _preferenceAuditHistoryQueryProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PreferenceAuditHistoryQuery(null, _customerService.Object, _preferenceAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new PreferenceAuditHistoryQuery(_logger.Object, null, _preferenceAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new PreferenceAuditHistoryQuery(_logger.Object, _customerService.Object, null));
        }

        [Fact]
        public async Task PreferenceAuditHistoryQueryShouldSucceedWithCustomerID()
        {
            var response = TestDataProvider.GetTestData<PreferenceAuditHistoryResponse>("Audit\\PreferenceAuditHistoryResponse");
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _preferenceAuditHistoryQueryProvider.Setup(m => m.GetPreferenceAuditHistory(customerId)).ReturnsAsync(response);
            var queryResponse = await _preferenceAuditHistoryQuery.RetrievePreferenceAuditHistory(customerId, null);

            Assert.IsType<PreferenceAuditHistoryResponse>(queryResponse);
            Assert.True(response.PreferenceHistoryAudit.Count() == queryResponse.PreferenceHistoryAudit.Count());
        }

        [Fact]
        public async Task PreferenceAuditHistoryQueryShouldSucceedWithMemberNumber()
        {
            var response = TestDataProvider.GetTestData<PreferenceAuditHistoryResponse>("Audit\\PreferenceAuditHistoryResponse");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            customerDTO.Customer.CustomerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _customerService.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _preferenceAuditHistoryQueryProvider.Setup(m => m.GetPreferenceAuditHistory("69b8f74b-e412-4a31-96e0-352b9603dfd3")).ReturnsAsync(response);
            var queryResponse = await _preferenceAuditHistoryQuery.RetrievePreferenceAuditHistory(null, "9106488951");

            Assert.IsType<PreferenceAuditHistoryResponse>(queryResponse);
            Assert.True(response.PreferenceHistoryAudit.Count() == queryResponse.PreferenceHistoryAudit.Count());
        }

        [Fact]
        public async Task PreferenceAuditHistoryQueryShouldThrowException()
        {
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _preferenceAuditHistoryQueryProvider.Setup(m => m.GetPreferenceAuditHistory(customerId)).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _preferenceAuditHistoryQuery.RetrievePreferenceAuditHistory(customerId, null));
        }

        [Fact]
        public async Task PreferenceAuditHistoryQueryShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _preferenceAuditHistoryQuery.RetrievePreferenceAuditHistory(null, null));
        }
    }
}