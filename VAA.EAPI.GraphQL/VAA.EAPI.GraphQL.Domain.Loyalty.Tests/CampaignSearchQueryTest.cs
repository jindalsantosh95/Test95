using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignSearchQueryTest
    {
        private readonly Mock<ILogger<CampaignSearchQuery>> _logger;
        private readonly CampaignSearchQuery _campaignSearchQuery;
        private readonly Mock<ICampaignSearchQueryProvider> _provider;

        public CampaignSearchQueryTest()
        {
            _logger = new Mock<ILogger<CampaignSearchQuery>>();
            _campaignSearchQuery = new CampaignSearchQuery(_logger.Object);
            _provider = new Mock<ICampaignSearchQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignSearchQuery(null));
        }

        [Fact]
        public async Task CampaignSearchShouldSucceed()
        {
            CampaignSearchRequest req = TestDataProvider.GetTestData<CampaignSearchRequest>("Loyalty\\CampaignSearchRequest");
            var response = TestDataProvider.GetTestData<List<CampaignSearchData>>("Loyalty\\CampaignSearchResponse");

            _provider.Setup(m => m.Query(req)).ReturnsAsync(response);

            var queryResponse = await _campaignSearchQuery.CampaignSearch(req, _provider.Object, _logger.Object);
            Assert.IsType<List<CampaignSearchData>>(queryResponse);
            Assert.True(response.Count == queryResponse.Count);
        }


        [Fact]
        public async Task CampaignSearchShouldThrowException()
        {
            CampaignSearchRequest req = TestDataProvider.GetTestData<CampaignSearchRequest>("Loyalty\\CampaignSearchRequest");
            var response = TestDataProvider.GetTestData<List<CampaignSearchData>>("Loyalty\\CampaignSearchResponse");

            _provider.Setup(m => m.Query(req)).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _campaignSearchQuery.CampaignSearch(req, _provider.Object, _logger.Object));
        }

        [Fact]
        public async Task CampaignSearchShouldThrowNullException()
        {
            CampaignSearchRequest req = TestDataProvider.GetTestData<CampaignSearchRequest>("Loyalty\\CampaignSearchRequest");
            await Assert.ThrowsAsync<ArgumentNullException>(() => _campaignSearchQuery.CampaignSearch(req, null, _logger.Object));
        }
    }
}
