using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignRetrieveQueryTest
    {
        private readonly Mock<ILogger<CampaignRetrieveQuery>> _logger;
        private readonly CampaignRetrieveQuery _campaignRetrieveQuery;
        private readonly Mock<ICampaignRetrieveQueryProvider> _provider;

        public CampaignRetrieveQueryTest()
        {
            _logger = new Mock<ILogger<CampaignRetrieveQuery>>();
            _campaignRetrieveQuery = new CampaignRetrieveQuery(_logger.Object);
            _provider = new Mock<ICampaignRetrieveQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullLogger()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignRetrieveQuery(null));
        }

        [Fact]
        public async Task CampaignRetrieveShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<CampaignRetrieveData>("Loyalty\\CampaignRetrieveResponse");

            _provider.Setup(m => m.Query(null, It.IsAny<string>())).ReturnsAsync(response);

            var queryResponse = await _campaignRetrieveQuery.CampaignRetrieve(null, "XYZ", _provider.Object, _logger.Object);
            Assert.IsType<CampaignRetrieveData>(queryResponse);
            Assert.True(response.AssociatedMemberCount >= 0);
        }

        [Fact]
        public async Task CampaignRetrieveShouldThrowException()
        {
            var response = TestDataProvider.GetTestData<CampaignRetrieveData>("Loyalty\\CampaignRetrieveResponse");

            _provider.Setup(m => m.Query(null, It.IsAny<string>())).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _campaignRetrieveQuery.CampaignRetrieve(null, "XYZ", _provider.Object, _logger.Object));
        }

        [Fact]
        public async Task CampaignRetrieveShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _campaignRetrieveQuery.CampaignRetrieve(null, "XYZ", null, _logger.Object));
            await Assert.ThrowsAsync<ArgumentException>(() => _campaignRetrieveQuery.CampaignRetrieve(null, null, _provider.Object, _logger.Object));
        }
    }
}
