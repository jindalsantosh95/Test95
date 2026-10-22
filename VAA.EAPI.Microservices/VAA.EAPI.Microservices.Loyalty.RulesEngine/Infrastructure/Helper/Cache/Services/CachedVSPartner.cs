using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Helper.Cache.Services
{
    public class CachedVSPartner : ICachedVSPartner
    {
        private readonly IMemoryCache _cache;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public CachedVSPartner(IMemoryCache cache, ICommonRefDBRepository commonRefDBRepository)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }

        public async Task<string> GetCachedVSPartnerData(string partnerCode)
        {
            string partnerId;

            //Using SemaphoreSlim here, as lock doesn't allow async calls
            await semaphoreSlim.WaitAsync();

            try
            {
                _cache.TryGetValue(Constants.Application.VSPartnerIdCacheKey, out partnerId);
                if (partnerId == null)
                {
                    TimeSpan CommonCachingSlidingExpiration = TimeSpan.Parse(ConfigReader.Get(Constants.EnvironmentSettings.CommonCachingSlidingExpiration));
                    TimeSpan CommonCachingAbsoluteExpiration = TimeSpan.Parse(ConfigReader.Get(Constants.EnvironmentSettings.CommonCachingAbsoluteExpiration));
                    partnerId = await _commonRefDBRepository.GetPartnerId(partnerCode);
                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(CommonCachingSlidingExpiration).SetAbsoluteExpiration(CommonCachingAbsoluteExpiration);
                    _cache.Set(Constants.Application.VSPartnerIdCacheKey, partnerId, cacheEntryOptions);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return partnerId;

        }
    }
}
