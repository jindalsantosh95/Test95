using System;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Caching
{
    public interface IDistributedCacheService
    {
        Task<T> Get<T>(string key);
        Task Set<T>(string key, T value, bool isSlidingExpiration, TimeSpan slidingExpirationTime, TimeSpan absoluteExpirationTime);
        Task Refresh(string key);
        Task Remove(string key);
    }
}
