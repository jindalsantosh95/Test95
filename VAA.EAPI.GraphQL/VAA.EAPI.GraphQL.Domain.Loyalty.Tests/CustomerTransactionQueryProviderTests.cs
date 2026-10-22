using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CustomerTransactionQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;

        public CustomerTransactionQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerTransactionQueryProvider(null, _mockConnector.Object));
            Assert.Throws<ArgumentNullException>(() => new CustomerTransactionQueryProvider(_mocktokenProvider.Object, null));
        }

        [Fact]
        public async Task GetAwardDetailsShouldSucceed()
        {
            var awardDetailsResponse = TestDataProvider.GetTestData<APIResponse<AwardResponse>>("Loyalty\\AwardDetails_APIResponse");

            _mockConnector.Setup(m => m.GetAsync<AwardResponse>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(awardDetailsResponse);

            var provider = new CustomerTransactionQueryProvider(_mocktokenProvider.Object, _mockConnector.Object);
            var queryResponse = await provider.GetAwardDetails("D971F4AE-D22E-84D1-E053-6941D90A4EF2");

            Assert.NotNull(queryResponse);
            Assert.Equal("D971F4AE-D22E-84D1-E053-6941D90A4EF2", awardDetailsResponse.Response?.Awards?.FirstOrDefault()?.CustomerId);
            Assert.Equal(queryResponse.Awards?.FirstOrDefault()?.AwardId, awardDetailsResponse.Response?.Awards?.FirstOrDefault()?.AwardId);
            Assert.Equal(queryResponse.Awards?.FirstOrDefault()?.ProductId, awardDetailsResponse.Response?.Awards?.FirstOrDefault()?.ProductId);
        }
    }
}
