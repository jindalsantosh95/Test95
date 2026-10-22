using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PromotionSearchQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public PromotionSearchQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new PromotionSearchQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new PromotionSearchQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task QueryShouldSucceed()
        {
            APIResponse<PromotionSearchResponse> response = TestDataProvider.GetTestData<APIResponse<PromotionSearchResponse>>("PromotionSearch\\PromotionSearch_APIResponse");
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<PromotionSearchResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new PromotionSearchQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.Query("1-19J90XO", null, null, null, null, null, null, null);
            Assert.IsType<List<PromotionSearchData>>(queryResponse);
            Assert.True(response.Response.Promotions.Count == queryResponse.Count);
        }
    }
}
