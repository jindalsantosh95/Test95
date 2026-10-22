using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Infrastructure.Caching;

namespace VAA.EAPI.Microservices.Tests
{
    public sealed class DistributedCacheTests
    {
        [Fact]
        public void ExpireDistributedCache()
        {
            var distributedCache = new DistributedCache();
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(1)
            };

            distributedCache.Set(nameof(ExpireDistributedCache), [1], options);

            Thread.Sleep(500);

            var entry = distributedCache.Get(nameof(ExpireDistributedCache)) as byte[];

            Assert.NotNull(entry);
            Assert.IsType<byte[]>(entry);
            Assert.Single(entry);
            Assert.Equal(1, entry[0]);

            Thread.Sleep(1000);

            entry = distributedCache.Get(nameof(ExpireDistributedCache)) as byte[];

            Assert.Null(entry);
        }

        [Fact]
        public async Task ExpireDistributedCacheService()
        {
            var distributedCacheService = new DistributedCacheService(new DistributedCache(), new Mock<ILogger<DistributedCacheService>>().Object);

            await distributedCacheService.Set(nameof(ExpireDistributedCache), new byte[] { 1 }, false, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            Thread.Sleep(500);

            var entry = await distributedCacheService.Get<byte[]>(nameof(ExpireDistributedCache));

            Assert.NotNull(entry);
            Assert.IsType<byte[]>(entry);
            Assert.Single(entry);
            Assert.Equal(1, entry[0]);

            Thread.Sleep(2000);

            entry = await distributedCacheService.Get<byte[]>(nameof(ExpireDistributedCache));

            Assert.Null(entry);
        }
    }
}
