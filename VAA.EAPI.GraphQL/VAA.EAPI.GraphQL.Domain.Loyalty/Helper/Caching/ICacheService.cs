using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Helper.Caching
{
    public interface ICacheService
    {
        Task<T> GetCacheValue<T>(string key);
        Task SetCacheValue<T>(string key, T value, bool IsSlidingExpiration);
    }
}
