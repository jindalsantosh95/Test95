using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common;
using Environment = VAA.EAPI.Microservices.Retail.Order.Payment.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper
{
    public class CachedChannel : ICachedChannel
    {
        private readonly ITableStorageService<Channel> _tableStorageService;
        private readonly IMemoryCache _cache;

        public CachedChannel(ITableStorageService<Channel> tableStorageService, IMemoryCache cache)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = Constants.Application.ConfigTableName;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IEnumerable<Channel> GetChannelEntities()
        {
            _cache.TryGetValue(Constants.Application.CacheKey, out IEnumerable<Channel> channelEntitiy);

            return channelEntitiy;
        }

        private async Task SetChannelEntity()
        {
            var channelEntities = await _tableStorageService.RetrieveItemsAsync(default);

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(ConfigReader.Try<double>(Environment.CommonCachingSlidingExpirationInDay, 1)))
                .SetAbsoluteExpiration(TimeSpan.FromDays(ConfigReader.Try<double>(Environment.CommonCachingAbsoluteExpirationInDay, 1)));

            _cache.Set(Constants.Application.CacheKey, channelEntities, cacheEntryOptions);
        }

        public async Task<Channel> GetChannelEntity(string channelId)
        {
            IEnumerable<Channel> cachedChannelEntities = GetChannelEntities();

            if (cachedChannelEntities == null)
            {
                await SetChannelEntity();

                cachedChannelEntities = GetChannelEntities();
            }

            return cachedChannelEntities.FirstOrDefault(x => x.RowKey.Equals(channelId));
        }
    }
}
