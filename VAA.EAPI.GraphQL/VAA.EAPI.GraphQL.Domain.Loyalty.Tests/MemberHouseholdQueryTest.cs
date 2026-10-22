using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class MemberHouseholdQueryTest
    {
        private readonly Mock<ILogger<MemberHouseholdQuery>> _logger;
        private readonly MemberHouseholdQuery _memberHouseholdQuery;
        private readonly Mock<IMemberHouseholdQueryProvider> _provider;
        private readonly Mock<ICustomerProvider> _customerProvider;

        public MemberHouseholdQueryTest()
        {
            _logger = new Mock<ILogger<MemberHouseholdQuery>>();
            _memberHouseholdQuery = new MemberHouseholdQuery(_logger.Object);
            _customerProvider = new Mock<ICustomerProvider>();
            _provider = new Mock<IMemberHouseholdQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberHouseholdQuery(null));
        }

        [Fact]
        public async Task MemberHouseholdShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<MemberHouseholdResponse>("Loyalty\\MemberHouseholdResponse");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            var customer = customerDTO.Customer;
            _provider.Setup(m => m.Query("D971F4AC-FEF6-84D1-E053-6941D90A4EF2")).ReturnsAsync(response);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            var queryResponse = await _memberHouseholdQuery.MemberHousehold("D971F4AC-FEF6-84D1-E053-6941D90A4EF2", null, _customerProvider.Object, _provider.Object, _logger.Object);
            Assert.IsType<MemberHouseholdResponse>(queryResponse);
            Assert.All(queryResponse.HouseholdMembers, householdMember => Assert.Equal("Linked Member", householdMember.RelationshipType));
        }

        [Fact]
        public async Task MemberHouseholdShouldThrowException()
        {
            var response = TestDataProvider.GetTestData<MemberHouseholdResponse>("Loyalty\\PromotionEnrolledMembersResponse");
            _provider.Setup(m => m.Query("D971F4AC-FEF6-84D1-E053-6941D90A4EF2")).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _memberHouseholdQuery.MemberHousehold("D971F4AC-FEF6-84D1-E053-6941D90A4EF2", null, _customerProvider.Object, _provider.Object, _logger.Object));
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task MemberHouseholdShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _memberHouseholdQuery.MemberHousehold(null, null, _customerProvider.Object, _provider.Object, _logger.Object));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _memberHouseholdQuery.MemberHousehold(null, "1-XXXX", _customerProvider.Object, _provider.Object, _logger.Object));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _memberHouseholdQuery.MemberHousehold("D971F4AC-FEF6-84D1-E053-6941D90A4EF2", "1-XXXX", _customerProvider.Object, null, _logger.Object));
        }
    }
}

