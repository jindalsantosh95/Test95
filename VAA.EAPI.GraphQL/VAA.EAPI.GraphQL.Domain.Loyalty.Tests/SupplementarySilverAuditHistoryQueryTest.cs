using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class SupplementarySilverAuditHistoryQueryTest
    {
        private readonly Mock<ICustomerProvider> _customerService;
        private readonly Mock<ISupplementarySilverAuditHistoryQueryProvider> _supplementarySilverAuditHistoryQueryProvider;
        private readonly Mock<ILogger<SupplementarySilverAuditHistoryQuery>> _logger;
        private readonly SupplementarySilverAuditHistoryQuery _supplementarySilverAuditHistoryQuery;

        public SupplementarySilverAuditHistoryQueryTest()
        {
            _customerService = new Mock<ICustomerProvider>();
            _supplementarySilverAuditHistoryQueryProvider = new Mock<ISupplementarySilverAuditHistoryQueryProvider>();
            _logger = new Mock<ILogger<SupplementarySilverAuditHistoryQuery>>();
            _supplementarySilverAuditHistoryQuery = new SupplementarySilverAuditHistoryQuery(_logger.Object, _customerService.Object, _supplementarySilverAuditHistoryQueryProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SupplementarySilverAuditHistoryQuery(null, _customerService.Object, _supplementarySilverAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SupplementarySilverAuditHistoryQuery(_logger.Object, null, _supplementarySilverAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SupplementarySilverAuditHistoryQuery(_logger.Object, _customerService.Object, null));
        }

        [Fact]
        public async Task SupplementarySilverAuditHistoryQueryShouldSucceedWithCustomerID()
        {
            var response = TestDataProvider.GetTestData<SupplementarySilverAuditHistoryResponse>("Audit\\SupplementarySilverAuditHistoryQueryResponse");
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _supplementarySilverAuditHistoryQueryProvider.Setup(m => m.GetSupplementarySilverAuditHistory(customerId)).ReturnsAsync(response);
            var queryResponse = await _supplementarySilverAuditHistoryQuery.RetrieveSupplementarySilverAuditHistory(customerId, null);

            Assert.IsType<SupplementarySilverAuditHistoryResponse>(queryResponse);
            Assert.True(response.SupplementarySilverAuditHistory.Count() == queryResponse.SupplementarySilverAuditHistory.Count());
        }

        [Fact]
        public async Task SupplementarySilverAuditHistoryQueryShouldSucceedWithMemberNumber()
        {
            var response = TestDataProvider.GetTestData<SupplementarySilverAuditHistoryResponse>("Audit\\SupplementarySilverAuditHistoryQueryResponse");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            customerDTO.Customer.CustomerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _customerService.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _supplementarySilverAuditHistoryQueryProvider.Setup(m => m.GetSupplementarySilverAuditHistory("69b8f74b-e412-4a31-96e0-352b9603dfd3")).ReturnsAsync(response);
            var queryResponse = await _supplementarySilverAuditHistoryQuery.RetrieveSupplementarySilverAuditHistory(null, "9106488951");

            Assert.IsType<SupplementarySilverAuditHistoryResponse>(queryResponse);
            Assert.True(response.SupplementarySilverAuditHistory.Count() == queryResponse.SupplementarySilverAuditHistory.Count());
        }

        [Fact]
        public async Task SupplementarySilverAuditHistoryQueryShouldThrowException()
        {
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _supplementarySilverAuditHistoryQueryProvider.Setup(m => m.GetSupplementarySilverAuditHistory(customerId)).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _supplementarySilverAuditHistoryQuery.RetrieveSupplementarySilverAuditHistory(customerId, null));
        }

        [Fact]
        public async Task SupplementarySilverAuditHistoryQueryShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _supplementarySilverAuditHistoryQuery.RetrieveSupplementarySilverAuditHistory(null, null));
        }
    }
}