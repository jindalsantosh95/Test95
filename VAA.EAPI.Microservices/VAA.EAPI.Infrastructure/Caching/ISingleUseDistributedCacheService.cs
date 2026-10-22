using System;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Caching
{
    // Additive interface: existing cache implementations and consumers need not change.
    public interface ISingleUseDistributedCacheService : IDistributedCacheService
    {
        Task<DateTime?> SetAsync<T>(string key, T value, TimeSpan lifetime,
            CacheMode mode = CacheMode.Default, SingleUseCacheOptions options = null,
            CancellationToken cancellationToken = default);
        Task<T> TryConsumeAsync<T>(string key, string scopeKey, string binding,
            CancellationToken cancellationToken = default) where T : class;
    }
}
