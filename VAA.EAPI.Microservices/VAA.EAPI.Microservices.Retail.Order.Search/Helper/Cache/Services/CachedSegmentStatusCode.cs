using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Services
{
    public class CachedSegmentStatusCode : ICachedSegmentStatusCode
    {
        private readonly ICacheService _iCacheService;
        private readonly TableStorageService<SegmentStatusCodeDataEntity> _tableStorageService;
        public CachedSegmentStatusCode(ICacheService iCacheService, ITableStorageService<SegmentStatusCodeDataEntity> tableStorageService)
        {
            _iCacheService = iCacheService;
            _tableStorageService = (TableStorageService<SegmentStatusCodeDataEntity>)tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.StatusCodeTableName);
        }

        public async Task<string> GetStatusCodeDescriptionAsync(string statusCode)
        {
            string description = await _iCacheService.GetCacheValue<string>(statusCode);
            if (string.IsNullOrEmpty(description))
            {
                var result = await _tableStorageService.RetrieveAsync(Constants.Application.StatusCodePartitionKey, statusCode);
                description = result?.Description;
                await _iCacheService.SetCacheValue(statusCode, description, true);
            }
            return description;
        }
    }
}
