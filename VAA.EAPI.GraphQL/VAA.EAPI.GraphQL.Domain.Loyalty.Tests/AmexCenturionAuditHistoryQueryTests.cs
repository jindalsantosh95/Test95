using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class AmexCenturionAuditHistoryQueryTests
    {
        private readonly Mock<ICustomerProvider> _customerService;
        private readonly Mock<IAmexCenturionAuditHistoryQueryProvider> _amexCenturionAuditHistoryQueryProvider;
        private readonly Mock<ILogger<AmexCenturionAuditHistoryQuery>> _logger;
        private readonly AmexCenturionAuditHistoryQuery _amexCenturionAuditHistoryQuery;

        public AmexCenturionAuditHistoryQueryTests()
        {
            _customerService = new Mock<ICustomerProvider>();
            _amexCenturionAuditHistoryQueryProvider = new Mock<IAmexCenturionAuditHistoryQueryProvider>();
            _logger = new Mock<ILogger<AmexCenturionAuditHistoryQuery>>();
            _amexCenturionAuditHistoryQuery = new AmexCenturionAuditHistoryQuery(_logger.Object, _customerService.Object, _amexCenturionAuditHistoryQueryProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenInputObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AmexCenturionAuditHistoryQuery(null, _customerService.Object, _amexCenturionAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new AmexCenturionAuditHistoryQuery(_logger.Object, null, _amexCenturionAuditHistoryQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new AmexCenturionAuditHistoryQuery(_logger.Object, _customerService.Object, null));
        }

        [Fact]
        public async Task AmexCenturioneAuditHistoryQueryShouldSucceedWithCustomerID()
        {
            var mockResponse = TestDataProvider.GetTestData<AmexCenturionAuditHistoryResponse>("Audit\\AmexCenturionAuditHistoryResponse");
            var customerId = "D92252A8-5198-6F04-E053-7D41D70A1575";

            _amexCenturionAuditHistoryQueryProvider.Setup(m => m.GetAmexCenturionAuditHistory(customerId)).ReturnsAsync(mockResponse);
            var queryResponse = await _amexCenturionAuditHistoryQuery.RetrieveAmexCenturionAuditHistory(customerId, null);

            Assert.IsType<AmexCenturionAuditHistoryResponse>(queryResponse);
            Assert.True(mockResponse.AmexCenturionAuditHistory.Count() == queryResponse.AmexCenturionAuditHistory.Count());
            Assert.Equal(mockResponse.AmexCenturionAuditHistory.First().CardLinkingDocID, queryResponse.AmexCenturionAuditHistory.First().CardLinkingDocID);
            Assert.Equal(mockResponse.AmexCenturionAuditHistory.First().PartnerCode, queryResponse.AmexCenturionAuditHistory.First().PartnerCode);
        }

        [Fact]
        public async Task AmexCenturionAuditHistoryQueryShouldSucceedWithMemberNumber()
        {
            var mockResponse = TestDataProvider.GetTestData<AmexCenturionAuditHistoryResponse>("Audit\\AmexCenturionAuditHistoryResponse");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            customerDTO.Customer.CustomerId = "D92252A8-5198-6F04-E053-7D41D70A1575";

            _customerService.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _amexCenturionAuditHistoryQueryProvider.Setup(m => m.GetAmexCenturionAuditHistory("D92252A8-5198-6F04-E053-7D41D70A1575")).ReturnsAsync(mockResponse);
            var queryResponse = await _amexCenturionAuditHistoryQuery.RetrieveAmexCenturionAuditHistory(null, "9106488951");

            Assert.IsType<AmexCenturionAuditHistoryResponse>(queryResponse);
            Assert.True(mockResponse.AmexCenturionAuditHistory.Count() == queryResponse.AmexCenturionAuditHistory.Count());
            Assert.Equal(mockResponse.AmexCenturionAuditHistory.First(), queryResponse.AmexCenturionAuditHistory.First());
            Assert.Equal(mockResponse.AmexCenturionAuditHistory.First().CardLinkingUpdatedBy, queryResponse.AmexCenturionAuditHistory.First().CardLinkingUpdatedBy);
        }


        [Fact]
        public async Task AmexCenturionAuditHistoryQueryShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _amexCenturionAuditHistoryQuery.RetrieveAmexCenturionAuditHistory(null, null));
        }
    }
}
