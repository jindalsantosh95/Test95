using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Services
{
    public class CachedCampaignDetailsById : ICachedCampaignDetailsById
    {
        private readonly IMemoryCache _cache;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public CachedCampaignDetailsById(IMemoryCache cache, ICommonRefDBRepository commonRefDBRepository)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }

        public async Task<CampaignDetails> GetCachedCampaignDetailsById(string campaignId)
        {
            CampaignDetails campaignDetails;

            //Using SemaphoreSlim here, as lock doesn't allow async calls
            await semaphoreSlim.WaitAsync();

            try
            {
                _cache.TryGetValue(Constants.Application.CampaignDetailsCacheKey, out campaignDetails);
                if (campaignDetails == null || campaignDetails.CampaignId != campaignId)
                {
                    TimeSpan CommonCachingSlidingExpiration = ConfigReader.Get<TimeSpan>(Constants.Environment.CommonCachingSlidingExpiration);
                    TimeSpan CommonCachingAbsoluteExpiration = ConfigReader.Get<TimeSpan>(Constants.Environment.CommonCachingAbsoluteExpiration);
                    campaignDetails = await _commonRefDBRepository.GetCampaignDetails(campaignId);
                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(CommonCachingSlidingExpiration).SetAbsoluteExpiration(CommonCachingAbsoluteExpiration);
                    _cache.Set(Constants.Application.CampaignDetailsCacheKey, campaignDetails, cacheEntryOptions);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return campaignDetails;

        }

    }
}
