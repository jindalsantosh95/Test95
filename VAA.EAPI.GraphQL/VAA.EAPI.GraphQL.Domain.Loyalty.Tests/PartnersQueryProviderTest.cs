using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnersQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public PartnersQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnersQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new PartnersQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task QueryShouldSucceed()
        {
            APIResponse<PartnersResponse> response = TestDataProvider.GetTestData<APIResponse<PartnersResponse>>("PartnersRetrieval\\Partners_APIResponse");
            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<PartnersResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new PartnersQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.Query();
            Assert.IsType<List<PartnerProfile>>(queryResponse);
            Assert.True(response.Response.Partners.Count == queryResponse.Count);
        }
    }
}
