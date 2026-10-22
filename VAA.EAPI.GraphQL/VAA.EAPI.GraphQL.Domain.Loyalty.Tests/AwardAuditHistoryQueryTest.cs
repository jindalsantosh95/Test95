using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class AwardAuditHistoryQueryTest
    {
        private readonly Mock<ICustomerProvider> _customerService;
        private readonly Mock<IAwardAuditHistoryQueryProvider> _awardAuditHistoryQueryProvider;
        private readonly Mock<ILogger<AwardAuditHistoryQuery>> _logger;
        private readonly AwardAuditHistoryQuery _awardAuditHistoryQuery;

        public AwardAuditHistoryQueryTest()
        {
            _customerService = new Mock<ICustomerProvider>();
            _awardAuditHistoryQueryProvider = new Mock<IAwardAuditHistoryQueryProvider>();
            _logger = new Mock<ILogger<AwardAuditHistoryQuery>>();
            _awardAuditHistoryQuery = new AwardAuditHistoryQuery(_logger.Object, _customerService.Object, _awardAuditHistoryQueryProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AwardAuditHistoryQuery(null, _customerService.Object, _awardAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new AwardAuditHistoryQuery(_logger.Object, null, _awardAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new AwardAuditHistoryQuery(_logger.Object, _customerService.Object, null));
        }

        [Fact]
        public async Task AwardeAuditHistoryQueryShouldSucceedWithCustomerID()
        {
            var response = TestDataProvider.GetTestData<AwardAuditHistoryResponse>("Audit\\AwardAuditHistoryResponse");
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _awardAuditHistoryQueryProvider.Setup(m => m.GetAwardAuditHistory(customerId)).ReturnsAsync(response);
            var queryResponse = await _awardAuditHistoryQuery.RetrieveAwardAuditHistory(customerId, null);

            Assert.IsType<AwardAuditHistoryResponse>(queryResponse);
            Assert.True(response.AwardAuditHistory.Count() == queryResponse.AwardAuditHistory.Count());
        }

        [Fact]
        public async Task AwardAuditHistoryQueryShouldSucceedWithMemberNumber()
        {
            var response = TestDataProvider.GetTestData<AwardAuditHistoryResponse>("Audit\\AwardAuditHistoryResponse");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            customerDTO.Customer.CustomerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _customerService.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _awardAuditHistoryQueryProvider.Setup(m => m.GetAwardAuditHistory("69b8f74b-e412-4a31-96e0-352b9603dfd3")).ReturnsAsync(response);
            var queryResponse = await _awardAuditHistoryQuery.RetrieveAwardAuditHistory(null, "9106488951");

            Assert.IsType<AwardAuditHistoryResponse>(queryResponse);
            Assert.True(response.AwardAuditHistory.Count() == queryResponse.AwardAuditHistory.Count());
        }

        [Fact]
        public async Task PreferenceAuditHistoryQueryShouldThrowException()
        {
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _awardAuditHistoryQueryProvider.Setup(m => m.GetAwardAuditHistory(customerId)).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _awardAuditHistoryQuery.RetrieveAwardAuditHistory(customerId, null));
        }

        [Fact]
        public async Task PreferenceAuditHistoryQueryShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _awardAuditHistoryQuery.RetrieveAwardAuditHistory(null, null));
        }
    }
}