namespace VAA.EAPI.Web.Common.Infrastructure.Caching
{
    public interface ICacheService
    {
        Task<T> GetCacheValue<T>(string key);
        Task SetCacheValue<T>(string key, T value, bool IsSlidingExpiration);
        Task RemoveCacheValue<T>(string key);
    }
}
