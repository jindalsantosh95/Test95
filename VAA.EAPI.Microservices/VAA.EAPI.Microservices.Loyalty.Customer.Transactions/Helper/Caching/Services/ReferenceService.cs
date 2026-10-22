using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Services
{
    public class ReferenceService : IReferenceService
    {
        private readonly ReferenceCacheHelper _cacheHelper;
        private readonly ITableStorageService<ReferenceDetails> _tableStorage;

        public ReferenceService(ReferenceCacheHelper cacheHelper, ITableStorageService<ReferenceDetails> tableStorage)
        {
            _cacheHelper = cacheHelper;
            _tableStorage = tableStorage;
            _tableStorage.ConnectionString = ConfigReader.Get(EnvironmentSettings.MicroServiceDBKey);
            _tableStorage.TableName = Constants.Application.ReferenceTableName;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            string cacheKey = "ALL_PRODUCTS";

            return await _cacheHelper.GetOrFetchAsync(cacheKey, async () =>
            {
                var allRows = await _tableStorage.RetrieveItemsAsync(default);

                return allRows
                    .Where(r => r.PartitionKey == Constants.Application.ProductPartitionKey)
                    .Select(r => new Product
                    {
                        ProductId = r.RowKey,
                        ProductName = r.ProductName,
                        ProductType = r.ProductType,
                        ProductDescription = r.ProductDescription,
                        ProductCategory = r.ProductCategory,
                        CategoryCode = r.CategoryCode
                    })
                    .ToList();
            });
        }

        public async Task<IEnumerable<Partner>> GetAllPartnersAsync()
        {
            string cacheKey = "ALL_PARTNERS";

            return await _cacheHelper.GetOrFetchAsync(cacheKey, async () =>
            {
                var allRows = await _tableStorage.RetrieveItemsAsync(default);

                return allRows
                    .Where(r => r.PartitionKey == Constants.Application.PartnerPartitionKey)
                    .Select(r => new Partner
                    {
                        PartnerId = r.RowKey,
                        PartnerName = r.PartnerName,
                        PartnerCode = r.PartnerCode,
                        PartnerType = r.PartnerType,
                        PartnerSubType = r.PartnerSubType
                    })
                    .ToList();
            });
        }
    }
}
