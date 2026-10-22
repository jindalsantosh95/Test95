using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class TtlRulesDataService : ITtlRulesData
    {
        private readonly TableStorageService<TtlRulesDbEntity> _tableStorageService;
        private readonly ICacheService _cacheService;

        public TtlRulesDataService(ITableStorageService<TtlRulesDbEntity> tableStorageService, ICacheService cacheService)
        {
            _tableStorageService = (TableStorageService<TtlRulesDbEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.TtlRulesDBTableName);
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public async Task<IEnumerable<TtlRulesDbEntity>> GetTtlRulesData()
        {
            var cachedData = await _cacheService.GetCacheValue<IEnumerable<TtlRulesDbEntity>>(Constants.Application.TtlRulesCacheKey);

            if (cachedData == null)
            {
                cachedData = await _tableStorageService.RetrieveItemsAsync(null);
                await _cacheService.SetCacheValue(Constants.Application.TtlRulesCacheKey, cachedData, true);
            }
            return cachedData;
        }
    }
}
