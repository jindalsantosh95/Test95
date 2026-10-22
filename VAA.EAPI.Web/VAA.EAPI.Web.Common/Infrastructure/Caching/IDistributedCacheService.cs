namespace VAA.EAPI.Web.Common.Infrastructure.Caching
{
    public interface IDistributedCacheService
    {
        Task<T> GetCacheData<T>(string key);
        Task SetCacheData<T>(string key, T value, bool IsSlidingExpiration, TimeSpan slidingExpirationTime, TimeSpan absoluteExpirationTime);
        Task RemoveCacheData(string key);
    }
}
