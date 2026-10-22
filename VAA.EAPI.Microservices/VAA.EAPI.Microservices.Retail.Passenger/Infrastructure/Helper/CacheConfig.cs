using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper
{
    public class CacheConfig : ICacheConfig
    {
        private readonly TableStorageService<ChannelEntity> _tableStorageService;
        private readonly IMemoryCache _cache;
        public CacheConfig(ITableStorageService<ChannelEntity> tableStorageService, IMemoryCache cache)
        {
            _tableStorageService = (TableStorageService<ChannelEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ConfigTableName);
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }


        public List<ChannelEntity> GetChannelEntities()
        {
            _cache.TryGetValue(Constants.Application.CacheKey, out List<ChannelEntity> channelEntitiy);
            return channelEntitiy;
        }
        private async Task SetChannelEntity()
        {
            var channelEntities = await _tableStorageService.RetrieveItemsAsync(default);
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
           .SetSlidingExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingSlidingExpirationInDay)))).SetAbsoluteExpiration(TimeSpan.FromDays(Convert.ToDouble(ConfigReader.Get(Constants.Environment.CommonCachingAbsoluteExpirationInDay))));
            _cache.Set(Constants.Application.CacheKey, channelEntities, cacheEntryOptions);
        }
        public async Task<ChannelEntity> GetChannelEntity(string channelId)
        {
            List<ChannelEntity> cachedChannelEntities = GetChannelEntities();
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
