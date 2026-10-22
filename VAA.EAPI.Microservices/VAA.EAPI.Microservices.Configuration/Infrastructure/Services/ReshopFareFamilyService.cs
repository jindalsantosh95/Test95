using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class ReshopFareFamilyService : IReshopFareFamilyService
    {
        private readonly TableStorageService<ReshopFareFamilyDBEntity> _tableStorageService;

        public ReshopFareFamilyService(ITableStorageService<ReshopFareFamilyDBEntity> tableStorageService)
        {
            _tableStorageService = (TableStorageService<ReshopFareFamilyDBEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.ReshopFareFamilyDBTableName);
        }

        public async Task<List<ReshopFareFamilyDBEntity>> GetReshopFareFamilies()
        {
            return await _tableStorageService.RetrieveItemsAsync(null);            
        }
    }
}
