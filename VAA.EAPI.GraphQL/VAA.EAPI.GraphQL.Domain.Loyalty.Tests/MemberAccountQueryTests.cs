using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class MemberAccountQueryTests
    {
        private readonly Mock<ILogger<MemberAccountQuery>> _logger;
        private readonly MemberAccountQuery _memberAccountQuery;
        private readonly Mock<IMemberAccountQueryProvider> _provider;
        private readonly Mock<ICustomerProvider> _customerProvider;
        private readonly Mock<ICustomerTransactionQueryProvider> _customerTransactionQueryProvider;

        public MemberAccountQueryTests()
        {
            _logger = new Mock<ILogger<MemberAccountQuery>>();
            _provider = new Mock<IMemberAccountQueryProvider>();
            _customerProvider = new Mock<ICustomerProvider>();
            _memberAccountQuery = new MemberAccountQuery(_logger.Object);
            _customerTransactionQueryProvider = new Mock<ICustomerTransactionQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberAccountQuery(null));
        }

        [Fact]
        public async Task MemberAccountQueryShouldReturnSucceedWithGoodResponse()
        {
            var response = TestDataProvider.GetTestData<MemberAccount>("MemberAccount\\MemberAccountResponse");
            var memberCustomAttributes = TestDataProvider.GetTestData<MemberCustomAttributesResponse>("MemberAccount\\MemberCustomAttributesResponse");
            var customerId = "D4718748-3560-E0ED-E053-6941D90AAD4B";
            _provider.Setup(m => m.Query(customerId)).ReturnsAsync(response);
            _customerTransactionQueryProvider.Setup(m => m.GetMemberCustomAttributes(customerId)).ReturnsAsync(memberCustomAttributes);
            var queryResponse = await _memberAccountQuery.MemberAccount(customerId, null, _customerProvider.Object, _provider.Object, _customerTransactionQueryProvider.Object, _logger.Object);
            Assert.IsType<MemberAccount>(queryResponse);
        }

        [Fact]
        public async Task MemberAccountQueryShouldThrowException()
        {
            var customerId = "";
            await Assert.ThrowsAsync<ArgumentException>(() => _memberAccountQuery.MemberAccount(customerId, null, _customerProvider.Object, _provider.Object, _customerTransactionQueryProvider.Object, _logger.Object));
        }

        [Fact]
        public async Task MemberAccountShouldThrowNullException()
        {
            var customerId = "D4718748-3560-E0ED-E053-6941D90AAD4B";
            await Assert.ThrowsAsync<ArgumentNullException>(() => _memberAccountQuery.MemberAccount(customerId, null, _customerProvider.Object, null, _customerTransactionQueryProvider.Object, _logger.Object));
        }
    }
}
