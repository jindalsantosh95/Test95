using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class MemberSearchPhoneQueryTests
    {
        private readonly Mock<IVredCardDetailsQueryProvider> _vredCardDetailsQueryProvider;
        private readonly Mock<ICustomerTransactionQueryProvider> _customerTransactionQueryProvider;
        private readonly Mock<ICustomerQueryProvider> _customerQueryProvider;
        private readonly Mock<ILogger<MemberSearchPhoneQuery>> _logger;
        private readonly MemberSearchPhoneQuery _memberSearchPhoneQuery;

        public MemberSearchPhoneQueryTests()
        {
            _vredCardDetailsQueryProvider = new Mock<IVredCardDetailsQueryProvider>();
            _customerTransactionQueryProvider = new Mock<ICustomerTransactionQueryProvider>();
            _customerQueryProvider = new Mock<ICustomerQueryProvider>();
            _logger = new Mock<ILogger<MemberSearchPhoneQuery>>();
            _memberSearchPhoneQuery = new MemberSearchPhoneQuery(_logger.Object, _vredCardDetailsQueryProvider.Object, _customerTransactionQueryProvider.Object, _customerQueryProvider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberSearchPhoneQuery(null, _vredCardDetailsQueryProvider.Object, _customerTransactionQueryProvider.Object, _customerQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberSearchPhoneQuery(_logger.Object, null, _customerTransactionQueryProvider.Object, _customerQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberSearchPhoneQuery(_logger.Object, _vredCardDetailsQueryProvider.Object, null, _customerQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberSearchPhoneQuery(_logger.Object, _vredCardDetailsQueryProvider.Object, _customerTransactionQueryProvider.Object, null));
        }

        [Fact]
        public async Task MemberSearchPhoneQueryShouldSucceed()
        {
            var customerSearch = TestDataProvider.GetTestData<APIResponse<IEnumerable<CustomerSearch>>>("Loyalty\\CustomerSearchDoc");
            var vredPointBalance = TestDataProvider.GetTestData<APIResponse<VRedResponse>>("Loyalty\\VredPointResponse");
            var fcmDocResponse = TestDataProvider.GetTestData<FlyingClubMemberAPIResponse>("Loyalty\\FCMDocResponse");
            var customerResponse = TestDataProvider.GetTestData<APIResponse<Customers>>("Audit\\GetCustomerByCustId");

            _customerQueryProvider.Setup(x => x.GetCustomerSearch(It.IsAny<string>())).ReturnsAsync(customerSearch.Response);
            _vredCardDetailsQueryProvider.Setup(m => m.VredPointBalance(It.IsAny<string>())).ReturnsAsync(vredPointBalance.Response);
            _customerTransactionQueryProvider.Setup(m => m.GetFlyingClubMember(It.IsAny<string>())).ReturnsAsync(fcmDocResponse);
            _customerQueryProvider.Setup(m => m.GetCustomerDoc(It.IsAny<string>())).ReturnsAsync(customerResponse.Response);

            var queryResponse = await _memberSearchPhoneQuery.MemberSearchByPhone("2111111111");

            Assert.NotNull(queryResponse);
            Assert.True(queryResponse.Any());
            Assert.Equal("1074714856", queryResponse.First().MemberNumber);
            Assert.Equal("2111111111", queryResponse.First().PhoneNumber);
            Assert.Equal("test222555@gmail.com", queryResponse.First().EmailAddress);
            Assert.Equal("Mr Hundredfiftyfour Mobiletest", queryResponse.First().EnvelopeSalutation);
            Assert.Equal("44985", queryResponse.First().MemberPointBalance);
            Assert.Equal("Silver", queryResponse.First().CurrentTier);
        }
    }
}
