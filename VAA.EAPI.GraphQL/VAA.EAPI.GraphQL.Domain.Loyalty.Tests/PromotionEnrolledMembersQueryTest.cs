using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PromotionEnrolledMembersQueryTest
    {
        private readonly Mock<ILogger<PromotionEnrolledMembersQuery>> _logger;
        private readonly PromotionEnrolledMembersQuery _promoEnrolMembersQuery;
        private readonly Mock<IPromotionEnrolledMembersQueryProvider> _provider;

        public PromotionEnrolledMembersQueryTest()
        {
            _logger = new Mock<ILogger<PromotionEnrolledMembersQuery>>();
            _promoEnrolMembersQuery = new PromotionEnrolledMembersQuery(_logger.Object);
            _provider = new Mock<IPromotionEnrolledMembersQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new PromotionEnrolledMembersQuery(null));
        }

        [Fact]
        public async Task PromotionEnrolledMembersShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<PromotionEnrolledMemberResponse>("Loyalty\\PromotionEnrolledMembersResponse");
            _provider.Setup(m => m.Query("1-19J90XO")).ReturnsAsync(response);
            var queryResponse = await _promoEnrolMembersQuery.EnrolledMembers("1-19J90XO", _provider.Object, _logger.Object);
            Assert.IsType<PromotionEnrolledMemberResponse>(queryResponse);
        }

        [Fact]
        public async Task PromotionEnrolledMembersShouldThrowException()
        {
            var response = TestDataProvider.GetTestData<PromotionEnrolledMemberResponse>("Loyalty\\PromotionEnrolledMembersResponse");
            _provider.Setup(m => m.Query("1-19J90XO")).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _promoEnrolMembersQuery.EnrolledMembers("1-19J90XO", _provider.Object, _logger.Object));
        }

        [Fact]
        public async Task PromotionEnrolledMembersShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _promoEnrolMembersQuery.EnrolledMembers(null, _provider.Object, _logger.Object));
        }
    }
}
