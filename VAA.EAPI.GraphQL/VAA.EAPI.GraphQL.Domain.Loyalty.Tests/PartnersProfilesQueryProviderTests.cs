using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Helper.Caching;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class PartnersProfilesQueryProviderTests
    {
        private readonly Mock<IOAuthTokenProvider> _mocktokenProvider;
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly Mock<ICacheService> _mockCache;

        public PartnersProfilesQueryProviderTests()
        {
            _mocktokenProvider = new Mock<IOAuthTokenProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _mockCache = new Mock<ICacheService>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() => new PartnersProfilesQueryProvider(null, _mockConnector.Object, _mockCache.Object));
            Assert.Throws<ArgumentNullException>(() => new PartnersProfilesQueryProvider(_mocktokenProvider.Object, null, _mockCache.Object));
            Assert.Throws<ArgumentNullException>(() => new PartnersProfilesQueryProvider(_mocktokenProvider.Object, _mockConnector.Object, null));
        }

        [Fact]
        public async Task PartnerProfileRetrievalShouldSucceedWhenNotCached()
        {
            var response = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _mockConnector.Setup(m => m.GetAsync<IEnumerable<PartnerProfileDetails>>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            _mockCache.Setup(x => x.GetCacheValue<IEnumerable<PartnerProfileDetails>>(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<PartnerProfileDetails>>(null));
            _mockCache.Setup(x => x.SetCacheValue(It.IsAny<string>(), It.IsAny<IEnumerable<PartnerProfileDetails>>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            var provider = new PartnersProfilesQueryProvider(_mocktokenProvider.Object, _mockConnector.Object, _mockCache.Object);
            var queryResponse = await provider.GetPartnerProfileDetails();

            Assert.NotNull(queryResponse);
            Assert.Equal(queryResponse.First().PartnerCode, response.Response.First().PartnerCode);
        }

        [Fact]
        public async Task PartnerProfileRetrievalShouldSucceedWhenCached()
        {
            var response = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");

            _mocktokenProvider.Setup(m => m.AcquireTokenAsync(It.IsAny<string>())).ReturnsAsync("abc");  // Dummy token

            _mockConnector.Setup(m => m.GetAsync<IEnumerable<PartnerProfileDetails>>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(response);

            _mockCache.Setup(x => x.GetCacheValue<IEnumerable<PartnerProfileDetails>>(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<PartnerProfileDetails>>(response.Response));
            _mockCache.Setup(x => x.SetCacheValue(It.IsAny<string>(), It.IsAny<IEnumerable<PartnerProfileDetails>>(), It.IsAny<bool>())).Returns(Task.CompletedTask);

            var provider = new PartnersProfilesQueryProvider(_mocktokenProvider.Object, _mockConnector.Object, _mockCache.Object);
            var queryResponse = await provider.GetPartnerProfileDetails();

            Assert.NotNull(queryResponse);
            Assert.Equal(queryResponse.First().PartnerCode, response.Response.First().PartnerCode);
        }
    }
}

