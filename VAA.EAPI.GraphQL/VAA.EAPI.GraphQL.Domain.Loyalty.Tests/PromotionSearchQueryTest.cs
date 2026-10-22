using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PromotionSearchQueryTest
    {
        private readonly Mock<ILogger<PromotionSearchQuery>> _logger;
        private readonly PromotionSearchQuery _promotionSearchQuery;
        private readonly Mock<IPromotionSearchQueryProvider> _provider;

        public PromotionSearchQueryTest()
        {
            _logger = new Mock<ILogger<PromotionSearchQuery>>();
            _promotionSearchQuery = new PromotionSearchQuery(_logger.Object);
            _provider = new Mock<IPromotionSearchQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new PromotionSearchQuery(null));
        }

        [Fact]
        public async Task PromotionSearchShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<List<PromotionSearchData>>("PromotionSearch\\PromotionSearchResponse");
            _provider.Setup(m => m.Query("1-19J90XO", null, null, null, null, null, null, null)).ReturnsAsync(response);
            var queryResponse = await _promotionSearchQuery.PromotionSearch("1-19J90XO", null, null, null, null, null, null, null, _provider.Object, _logger.Object);
            Assert.IsType<List<PromotionSearchData>>(queryResponse);
            Assert.True(response.Count == queryResponse.Count);
        }

        [Fact]
        public async Task PromotionSearchShouldThrowException()
        {
            var response = TestDataProvider.GetTestData<List<PromotionSearchData>>("PromotionSearch\\PromotionSearchResponse");
            _provider.Setup(m => m.Query("1-19J90XO", null, null, null, null, null, null, null)).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _promotionSearchQuery.PromotionSearch("1-19J90XO", null, null, null, null, null, null, null, _provider.Object, _logger.Object));
        }

        [Fact]
        public async Task PromotionSearchShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _promotionSearchQuery.PromotionSearch("1-19J90XO", null, null, null, null, null, null, null, null, _logger.Object));
        }
    }
}
