using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Services;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository.CommonRefDB.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class CachedVSPartnerTest
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly CachedVSPartner _cachedVSPartner;

        public CachedVSPartnerTest()
        {
            _mockCache = new Mock<IMemoryCache>();

            var cachEntry = Mock.Of<ICacheEntry>();

            _mockCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cachEntry);

            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();

            _cachedVSPartner = new CachedVSPartner(_mockCache.Object, _mockCommonRefDBRepository.Object);

            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CachedVSPartner(null, _mockCommonRefDBRepository.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CachedVSPartner(_mockCache.Object, null));
        }

        [Fact]
        public async Task GetCachedVSPartnerDataShouldSuccess()
        {
            _mockCommonRefDBRepository.Setup(x => x.GetPartnerId("CX")).ReturnsAsync("XXXX");

            var result = await _cachedVSPartner.GetCachedVSPartnerData("CX");

            Assert.True(result is not null, "Expected to find partner data");
            Assert.True(result.Is("xxxx"), "Expected partner data to be 'XXXX'");

        }
    }
}
