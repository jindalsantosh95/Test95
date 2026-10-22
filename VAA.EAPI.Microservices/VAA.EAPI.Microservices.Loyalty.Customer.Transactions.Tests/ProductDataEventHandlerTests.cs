using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class ProductDataEventHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public ProductDataEventHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("microservicedbkey", "DefaultEndpointsProtocol = https; AccountName = dummyaccount; AccountKey = dummyaccountkey1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef; TableEndpoint = https://dummyaccount.table.core.windows.net/");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IProductDataEventHandler, ProductDataEventHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .BuildServiceProvider();
        }

        [Fact]
        public async Task HandleAsync_ValidProducts_InsertsIntoStorage()
        {
            var handler = _serviceProvider.GetService<IProductDataEventHandler>()!;
            var table = _serviceProvider.GetService<ITableStorageService<ProductDetails>>()!;

            var message = new ProductDataMessage
            {
                Data = new ProductDataPayload
                {
                    Products = new List<Product>
                    {
                        new Product
                        {
                            ProductId = "123",
                            ProductName = "Percentage",
                            ProductType = "Voucher",
                            ProductDescription = "Applicable on eligible bookings",
                            ProductCategory = "A",
                            CategoryCode = "CAT1"
                        },
                        new Product
                        {
                            ProductId = "456",
                            ProductName = "FixedAmount",
                            ProductType = "Voucher",
                            ProductDescription = "Flat discount",
                            ProductCategory = "B",
                            CategoryCode = "CAT2"
                        }
                    }
                }
            };

            await handler.HandleAsync(message);

            var entity1 = await table.RetrieveAsync(Constants.Application.ProductPartitionKey, "123");
            Assert.NotNull(entity1);
            Assert.Equal("Percentage", entity1.ProductName);
            Assert.Equal("Voucher", entity1.ProductType);
            Assert.Equal("Applicable on eligible bookings", entity1.ProductDescription);
            Assert.Equal("A", entity1.ProductCategory);
            Assert.Equal("CAT1", entity1.CategoryCode);

            var entity2 = await table.RetrieveAsync(Constants.Application.ProductPartitionKey, "456");
            Assert.NotNull(entity2);
            Assert.Equal("FixedAmount", entity2.ProductName);
            Assert.Equal("Voucher", entity2.ProductType);
            Assert.Equal("Flat discount", entity2.ProductDescription);
            Assert.Equal("B", entity2.ProductCategory);
            Assert.Equal("CAT2", entity2.CategoryCode);
        }

        [Fact]
        public async Task HandleAsync_ProductIdMissing_DoesNothing()
        {
            var handler = _serviceProvider.GetService<IProductDataEventHandler>()!;
            var table = _serviceProvider.GetService<ITableStorageService<ProductDetails>>()!;

            var message = new ProductDataMessage
            {
                Data = new ProductDataPayload
                {
                    Products = new List<Product>
                    {
                        new Product
                        {
                            ProductId = "",
                            ProductName = "TestName",
                            ProductType = "Voucher",
                            ProductDescription = "Test description",
                            ProductCategory = "Test category",
                            CategoryCode = "Test code"
                        }
                    }
                }
            };

            await handler.HandleAsync(message);

            var entity = await table.RetrieveAsync(Constants.Application.ProductPartitionKey, "");
            Assert.Null(entity);
        }

        [Fact]
        public async Task HandleAsync_ProductsEmpty_ThrowsException()
        {
            var handler = _serviceProvider.GetService<IProductDataEventHandler>()!;

            var message = new ProductDataMessage
            {
                Data = new ProductDataPayload
                {
                    Products = new List<Product>()
                }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => handler.HandleAsync(message));

            Assert.Contains("Products", ex.Message);
        }
    }
}
