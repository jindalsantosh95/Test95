using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class AirportListService : IAirportListService
    {
        private readonly TableStorageService<AirportListDBEntity> _tableStorageService;

        public AirportListService(ITableStorageService<AirportListDBEntity> tableStorageService)
        {
            _tableStorageService = (TableStorageService<AirportListDBEntity>)(tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService)));
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.CurrencyDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.AirportListDBTableName);
        }

        public async Task<List<AirportListDBEntity>> GetairportDetails()
        {
            return await _tableStorageService.RetrieveItemsAsync(null);
        }
    }
}
