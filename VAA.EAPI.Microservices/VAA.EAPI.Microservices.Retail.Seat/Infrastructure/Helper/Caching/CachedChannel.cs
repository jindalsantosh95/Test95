using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Seat.Domain;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface;
using Environment = VAA.EAPI.Microservices.Retail.Seat.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Caching
{
    public class CachedChannel : ICachedChannel
    {
        private readonly TableStorageService<Channel> _tableStorageService;
        private readonly IMemoryCache _cache;

        public CachedChannel(ITableStorageService<Channel> tableStorageService, IMemoryCache cache)
        {
            _tableStorageService = (TableStorageService<Channel>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Environment.ReferenceDataMicroServiceDbKey);
            _tableStorageService.TableName = Constants.Application.ConfigTableName;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public List<Channel> GetChannelEntities()
        {
            _cache.TryGetValue(Constants.Application.CacheKey, out List<Channel> channelEntity);
            return channelEntity;
        }

        private async Task SetChannelEntity()
        {
            var channelEntities = await _tableStorageService.RetrieveItemsAsync(default);

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Environment.CommonCachingSlidingExpirationInDay)))
               .SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Get<double>(Environment.CommonCachingAbsoluteExpirationInDay)));

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

            return cachedChannelEntities.Where(x => x.RowKey.Equals(channelId))?.FirstOrDefault();
        }
    }
}
