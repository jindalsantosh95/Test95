using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Services;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CachedCampaignDetailsByIdTest
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly CachedCampaignDetailsById _cachedCampaignDetailsById;
        private readonly Mock<MemoryCacheEntryOptions> _mockCacheEntry;

        public CachedCampaignDetailsByIdTest()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _cachedCampaignDetailsById = new CachedCampaignDetailsById(_mockCache.Object, _mockCommonRefDBRepository.Object);
            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CachedCampaignDetailsById(null, _mockCommonRefDBRepository.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CachedCampaignDetailsById(_mockCache.Object, null));
        }

        [Fact]
        public async Task GetCachedCampaignDetailsByIdShouldThrowException()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            _mockCommonRefDBRepository.Setup(x => x.GetCampaignDetails(It.IsAny<string>())).ReturnsAsync(campaignData);
            await Assert.ThrowsAsync<NullReferenceException>(async () => await _cachedCampaignDetailsById.GetCachedCampaignDetailsById("XXXx"));
        }
    }
}
