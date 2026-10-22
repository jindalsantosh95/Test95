using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignRetrieveQueryProviderTest
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public CampaignRetrieveQueryProviderTest()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new CampaignRetrieveQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new CampaignRetrieveQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task QueryShouldSucceed()
        {
            APIResponse<CampaignRetrieveData> response = TestDataProvider.GetTestData<APIResponse<CampaignRetrieveData>>("Loyalty\\CampaignRetrieve_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token
            _mockConnector.Setup(m => m.GetAsync<CampaignRetrieveData>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            var provider = new CampaignRetrieveQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.Query(null, "XYZ");

            Assert.IsType<CampaignRetrieveData>(queryResponse);
            Assert.True(response.Response.AssociatedMemberCount >= 0);
        }
    }
}
