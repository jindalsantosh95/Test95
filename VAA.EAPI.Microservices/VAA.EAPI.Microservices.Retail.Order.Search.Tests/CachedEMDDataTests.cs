using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using Xunit;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Services;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class CachedEMDDataTests
    {
        private readonly Mock<IMemoryCache> _cache;
        private readonly Mock<ITableStorageService<RFICTableEntity>> _tableStorageService;

        public CachedEMDDataTests()
        {
            _cache = new Mock<IMemoryCache>();
            _tableStorageService = new Mock<ITableStorageService<RFICTableEntity>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCacheObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CachedRFICData(null, _tableStorageService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenTableServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CachedRFICData(_cache.Object, null));
        }

    }
}
