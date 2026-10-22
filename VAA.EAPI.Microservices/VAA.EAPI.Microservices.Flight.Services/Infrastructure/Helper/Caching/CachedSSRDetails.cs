using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Flight.Services.Domain;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Helper.Caching
{
    public class CachedSsrDetails : ICachedSsrDetails
    {

        private readonly TableStorageService<SsrDetails> _tableStorageService;
        private readonly IMemoryCache _cache;

        public CachedSsrDetails(ITableStorageService<SsrDetails> tableStorageService, IMemoryCache cache)
        {
            _tableStorageService = (TableStorageService<SsrDetails>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = Constants.Application.SSRTableName;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<List<SsrDetails>> GetSsrDetails()
        {
            List<SsrDetails> cachedSSREntities = GetSSREntities();
            if (cachedSSREntities == null)
            {
                await SetChannelEntity();
                cachedSSREntities = GetSSREntities();
            }

            return cachedSSREntities;
        }

        private List<SsrDetails> GetSSREntities()
        {
            _cache.TryGetValue(Constants.Application.CacheKey, out List<SsrDetails> sSREntitiy);

            return sSREntitiy;
        }

        private async Task SetChannelEntity()
        {
            var channelEntities = await _tableStorageService.RetrieveItemsAsync(default);

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingSlidingExpirationInDay)))
               .SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingAbsoluteExpirationInDay)));

            _cache.Set(Constants.Application.CacheKey, channelEntities, cacheEntryOptions);
        }
    }
}
