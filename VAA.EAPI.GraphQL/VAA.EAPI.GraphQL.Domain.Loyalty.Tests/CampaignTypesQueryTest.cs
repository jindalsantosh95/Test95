using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignTypesQueryTest
    {
        private readonly Mock<ILogger<CampaignTypesQuery>> _mockLogger;
        private readonly CampaignTypesQuery _campaignTypeQuery;
        private readonly Mock<ICampaignTypesQueryProvider> _provider;

        public CampaignTypesQueryTest()
        {
            _mockLogger = new Mock<ILogger<CampaignTypesQuery>>();
            _campaignTypeQuery = new CampaignTypesQuery(_mockLogger.Object);
            _provider = new Mock<ICampaignTypesQueryProvider>();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignTypesQuery(null));
        }

        [Fact]
        public async Task CampaignTypeRetrieveShouldSucceed()
        {
            var response = TestDataProvider.GetTestData<CampaignTypesResponse>("CampaignTypes\\CampaignTypeResponse");
            _provider.Setup(m => m.Query()).ReturnsAsync(response);
            var queryResponse = await _campaignTypeQuery.CampaignType(_provider.Object, _mockLogger.Object);
            Assert.IsType<CampaignTypesResponse>(queryResponse);
            Assert.True(response.CampaignTypes.Count == queryResponse.CampaignTypes.Count);
        }

        [Fact]
        public async Task CampaignTypeRetrieveShouldThrowException()
        {
            var response = TestDataProvider.GetTestData<CampaignTypesResponse>("CampaignTypes\\CampaignTypeResponse");
            _provider.Setup(m => m.Query()).Throws(new Exception());
            await Assert.ThrowsAsync<Exception>(() => _campaignTypeQuery.CampaignType(_provider.Object, _mockLogger.Object));
        }

        [Fact]
        public async void CampaignTypeRetrieveShouldThrowNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _campaignTypeQuery.CampaignType(null, _mockLogger.Object));
        }
    }
}
