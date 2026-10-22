using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Caching
{
    public interface ICacheService
    {
        Task<T?> GetCacheValue<T>(string key);
        Task SetCacheValue<T>(string key, T value, bool isSlidingExpiration);
        Task RemoveCacheValue(string key);
    }
}
