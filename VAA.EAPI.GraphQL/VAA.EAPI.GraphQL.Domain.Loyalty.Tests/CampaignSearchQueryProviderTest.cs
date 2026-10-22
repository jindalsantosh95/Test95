using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignSearchQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public CampaignSearchQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignSearchQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new CampaignSearchQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task QueryShouldSucceed()
        {
            CampaignSearchRequest req = TestDataProvider.GetTestData<CampaignSearchRequest>("Loyalty\\CampaignSearchRequest");
            APIResponse<CampaignSearchResponse> response = TestDataProvider.GetTestData<APIResponse<CampaignSearchResponse>>("Loyalty\\CampaignSearch_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<CampaignSearchResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new CampaignSearchQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.Query(req);

            Assert.IsType<List<CampaignSearchData>>(queryResponse);
            Assert.True(response.Response.Campaigns.Count == queryResponse.Count);
        }
    }
}
