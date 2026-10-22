using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class ContactHistoryQueryTest
    {
        private readonly Mock<ICustomerProvider> _customerService;
        private readonly Mock<IContactHistoryQueryProvider> _contactHistoryQueryService;
        private readonly Mock<ILogger<ContactHistoryQuery>> _logger;
        private readonly ContactHistoryQuery _contactHistoryQuery;

        public ContactHistoryQueryTest()
        {
            _customerService = new Mock<ICustomerProvider>();
            _contactHistoryQueryService = new Mock<IContactHistoryQueryProvider>();
            _logger = new Mock<ILogger<ContactHistoryQuery>>();
            _contactHistoryQuery = new ContactHistoryQuery(_logger.Object, _customerService.Object, _contactHistoryQueryService.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ContactHistoryQuery(null, _customerService.Object, _contactHistoryQueryService.Object));
            Assert.Throws<ArgumentNullException>(() => new ContactHistoryQuery(_logger.Object, null, _contactHistoryQueryService.Object));
            Assert.Throws<ArgumentNullException>(() => new ContactHistoryQuery(_logger.Object, _customerService.Object, null));
        }

        [Fact]
        public async Task ContatcHistoryQueryShouldSucceedWithCustomerID()
        {
            var response = TestDataProvider.GetTestData<ContactHistoryAuditResponse>("Audit\\ContactHistoryResponse");
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _contactHistoryQueryService.Setup(m => m.GetContactHistory(customerId)).ReturnsAsync(response);
            var queryResponse = await _contactHistoryQuery.RetrieveContactHistory(customerId, null);

            Assert.IsType<ContactHistoryAuditResponse>(queryResponse);
            Assert.True(response.ContactHistoryAudit.Count() == queryResponse.ContactHistoryAudit.Count());
        }

        [Fact]
        public async Task ContatcHistoryQueryShouldSucceedWithMemberNumber()
        {
            var response = TestDataProvider.GetTestData<ContactHistoryAuditResponse>("Audit\\ContactHistoryResponse");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            customerDTO.Customer.CustomerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _customerService.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _contactHistoryQueryService.Setup(m => m.GetContactHistory("69b8f74b-e412-4a31-96e0-352b9603dfd3")).ReturnsAsync(response);
            var queryResponse = await _contactHistoryQuery.RetrieveContactHistory(null, "9106488951");

            Assert.IsType<ContactHistoryAuditResponse>(queryResponse);
            Assert.True(response.ContactHistoryAudit.Count() == queryResponse.ContactHistoryAudit.Count());
        }

        [Fact]
        public async Task ContatcHistoryQueryShouldThrowException()
        {
            var customerId = "69b8f74b-e412-4a31-96e0-352b9603dfd3";

            _contactHistoryQueryService.Setup(m => m.GetContactHistory(customerId)).Throws(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _contactHistoryQuery.RetrieveContactHistory(customerId, null));
        }

        [Fact]
        public async Task ContatcHistoryQueryShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _contactHistoryQuery.RetrieveContactHistory(null, null));
        }
    }
}
