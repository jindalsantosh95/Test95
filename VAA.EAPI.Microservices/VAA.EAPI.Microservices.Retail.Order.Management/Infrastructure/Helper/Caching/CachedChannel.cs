using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Caching
{
    public class CachedChannel : ICachedChannel
    {
        private readonly TableStorageService<Channel> _tableStorageService;
        private readonly IMemoryCache _cache;
        public CachedChannel(ITableStorageService<Channel> tableStorageService, IMemoryCache cache)
        {
            _tableStorageService = (TableStorageService<Channel>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = Constants.Application.ConfigTableName;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public List<Channel> GetChannelEntities()
        {
            _cache.TryGetValue(Constants.Application.CacheKey, out List<Channel> channelEntitiy);
            return channelEntitiy;
        }
        private async Task SetChannelEntity()
        {
            var channelEntities = await _tableStorageService.RetrieveItemsAsync(default);
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingSlidingExpirationInDay)))
               .SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Constants.Environment.CommonCachingAbsoluteExpirationInDay)));
            _cache.Set(Constants.Application.CacheKey, channelEntities, cacheEntryOptions);
        }
        public async Task<Channel> GetChannelEntity(string channelId)
        {
            List<Channel> cachedChannelEntities = GetChannelEntities();
            if (cachedChannelEntities == null)
            {
                await SetChannelEntity();
                cachedChannelEntities = GetChannelEntities();
            }
            var channelItem = cachedChannelEntities.Where(x => x.RowKey.Equals(channelId))?.FirstOrDefault();
            return channelItem;
        }
    }
}
