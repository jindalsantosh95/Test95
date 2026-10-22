using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Search.Constants;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Services
{
    public class ProductName : IProductName
    {
        public async Task<string> GetProductName(string rowKey)
        {
            try
            {
                TableStorageService<ReferenceProductCodeDataEntity> _tableStorageService = new();
                _tableStorageService.ConnectionString = ConfigReader.Get(Environment.ReferenceDataMicroServiceDBKey);
                _tableStorageService.TableName = ConfigReader.Get(Environment.ProductCodeTableName);
                var response = await _tableStorageService.RetrieveAsync(Constants.Application.ProductCodePartitionKey, rowKey);
                return response?.ProductName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
