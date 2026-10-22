using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class CachedPartnersTest
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ICommonRefDBServices> _mockCommonRefDBServices;
        private readonly CachedPartners _cachedPartners;

        public CachedPartnersTest()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockCommonRefDBServices = new Mock<ICommonRefDBServices>();
            _cachedPartners = new CachedPartners(_mockCache.Object, _mockCommonRefDBServices.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CachedPartners(null, _mockCommonRefDBServices.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new CachedPartners(_mockCache.Object, null));
        }
    }
}
