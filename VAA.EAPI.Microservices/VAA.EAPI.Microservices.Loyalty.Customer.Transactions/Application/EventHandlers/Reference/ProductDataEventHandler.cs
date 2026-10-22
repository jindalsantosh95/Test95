using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference
{
    public class ProductDataEventHandler : IProductDataEventHandler
    {
        private readonly ITableStorageService<ProductDetails> _tableStorage;

        public ProductDataEventHandler(ITableStorageService<ProductDetails> tableStorage)
        {
            _tableStorage = tableStorage;
            _tableStorage.ConnectionString = ConfigReader.Get(Constants.EnvironmentSettings.MicroServiceDBKey);
            _tableStorage.TableName = Constants.Application.ReferenceTableName;
        }

        public async Task<string> HandleAsync(ProductDataMessage message)
        {
            try
            {
            
                if (message?.Data?.Products == null)
                    throw new ArgumentNullException(nameof(message.Data.Products));

                if (!message.Data.Products.Any())
                    throw new ArgumentException("Products collection cannot be empty.", nameof(message.Data.Products));

                foreach (var product in message.Data.Products)
                {
                    if (string.IsNullOrWhiteSpace(product.ProductId))
                        continue;

                    var entity = new ProductDetails
                    {
                        PartitionKey = Constants.Application.ProductPartitionKey,
                        RowKey = product.ProductId,
                        ProductName = product.ProductName,
                        ProductType = product.ProductType,
                        ProductDescription = product.ProductDescription,
                        ProductCategory = product.ProductCategory,
                        CategoryCode = product.CategoryCode
                    };

                    await _tableStorage.InsertOrMergeAsync(entity);

                }

                return "Product data processed successfully.";

            }

            catch (Exception ex)
            {
                throw new Exception($"Error processing reference data: {ex.Message}", ex);
            }
        }
    }
}
    


