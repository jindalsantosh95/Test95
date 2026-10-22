using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class CachedMemberAccountReferenceDataTest
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ICommonRefDBServices> _mockCommonRefDBServices;
        private readonly CachedMemberAccountReferenceData _cachedMemberAccountReferenceData;

        public CachedMemberAccountReferenceDataTest()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockCommonRefDBServices = new Mock<ICommonRefDBServices>();
            _cachedMemberAccountReferenceData = new CachedMemberAccountReferenceData(_mockCache.Object, _mockCommonRefDBServices.Object);
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
