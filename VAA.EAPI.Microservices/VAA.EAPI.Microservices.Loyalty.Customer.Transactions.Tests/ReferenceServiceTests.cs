using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class ReferenceServiceTests
    {
        private readonly IServiceProvider _provider;

        public ReferenceServiceTests()
        {
            ConfigReader.Set("microservicedbkey", "DefaultEndpointsProtocol=https;AccountName=dummy;AccountKey=dummy;TableEndpoint=https://dummy.table.core.windows.net/");
            ConfigReader.Set( Constants.EnvironmentSettings.CacheExpiryHours, "1");

            _provider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<ReferenceCacheHelper>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IReferenceService, ReferenceService>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnMappedProducts()
        {

            var table = _provider.GetRequiredService<ITableStorageService<ReferenceDetails>>();

            await table.InsertOrMergeAsync(new ReferenceDetails
            {
                PartitionKey = Constants.Application.ProductPartitionKey,
                RowKey = "P1",
                ProductName = "Voucher A",
                ProductType = "Electronic Voucher",
                ProductDescription = "Desc A",
                ProductCategory = "CatA"
            });

            await table.InsertOrMergeAsync(new ReferenceDetails
            {
                PartitionKey = Constants.Application.ProductPartitionKey,
                RowKey = "P2",
                ProductName = "Voucher B",
                ProductType = "Electronic Voucher",
                ProductDescription = "Desc B",
                ProductCategory = "CatB"
            });


            var service = _provider.GetRequiredService<IReferenceService>();

            var result = await service.GetAllProductsAsync();

            Assert.Equal(2, result.Count());

            var p1 = result.First(p => p.ProductId == "P1");
            Assert.Equal("Voucher A", p1.ProductName);
            Assert.Equal("Electronic Voucher", p1.ProductType);
            Assert.Equal("Desc A", p1.ProductDescription);
            Assert.Equal("CatA", p1.ProductCategory);

            var p2 = result.First(p => p.ProductId == "P2");
            Assert.Equal("Voucher B", p2.ProductName);
            Assert.Equal("Electronic Voucher", p2.ProductType);
            Assert.Equal("Desc B", p2.ProductDescription);
            Assert.Equal("CatB", p2.ProductCategory);
        }

        [Fact]
        public async Task GetAllPartnersAsync_ShouldReturnMappedPartners()
        {

            var table = _provider.GetRequiredService<ITableStorageService<ReferenceDetails>>();

            await table.InsertOrMergeAsync(new ReferenceDetails
            {
                PartitionKey = Constants.Application.PartnerPartitionKey,
                RowKey = "PR1",
                PartnerName = "Test Partner 1",
                PartnerCode = "A",
                PartnerType = "Airline",
                PartnerSubType = "Premium"
            });

            await table.InsertOrMergeAsync(new ReferenceDetails
            {
                PartitionKey = Constants.Application.PartnerPartitionKey,
                RowKey = "PR2",
                PartnerName = "Test Partner 2",
                PartnerCode = "B",
                PartnerType = "Airline",
                PartnerSubType = "Standard"
            });

            var service = _provider.GetRequiredService<IReferenceService>();

            var result = await service.GetAllPartnersAsync();

            Assert.Equal(2, result.Count());

            var pr1 = result.First(p => p.PartnerId == "PR1");
            Assert.Equal("Test Partner 1", pr1.PartnerName);
            Assert.Equal("A", pr1.PartnerCode);
            Assert.Equal("Airline", pr1.PartnerType);
            Assert.Equal("Premium", pr1.PartnerSubType);

            var pr2 = result.First(p => p.PartnerId == "PR2");
            Assert.Equal("Test Partner 2", pr2.PartnerName);
            Assert.Equal("B", pr2.PartnerCode);
            Assert.Equal("Airline", pr2.PartnerType);
            Assert.Equal("Standard", pr2.PartnerSubType);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldUseCacheOnSecondCall()
        {
            var table = _provider.GetRequiredService<ITableStorageService<ReferenceDetails>>();

            await table.InsertOrMergeAsync(new ReferenceDetails
            {
                PartitionKey = Constants.Application.ProductPartitionKey,
                RowKey = "P1",
                ProductName = "Voucher A",
                ProductType = "Electronic Voucher",
                ProductDescription = "Desc A",
                ProductCategory = "CatA"
            });

            var service = _provider.GetRequiredService<IReferenceService>();

            var firstCall = await service.GetAllProductsAsync();
            Assert.Single(firstCall);

            var entity = await table.RetrieveAsync(Constants.Application.ProductPartitionKey, "P1");
            await table.DeleteAsync(entity);

            var secondCall = await service.GetAllProductsAsync();

            Assert.Single(secondCall);
            Assert.Equal("P1", secondCall.First().ProductId);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldThrow_WhenTableStorageFails()
        {
            var mockTable = new Mock<ITableStorageService<ReferenceDetails>>();
            mockTable.Setup(t => t.RetrieveItemsAsync(default)).ThrowsAsync(new Exception("Storage failure"));

            var cache = new ReferenceCacheHelper(new MemoryCache(new MemoryCacheOptions()));

            var service = new ReferenceService(cache, mockTable.Object);

            var ex = await Assert.ThrowsAsync<Exception>(() => service.GetAllProductsAsync());
            Assert.Equal("Storage failure", ex.Message);
        }
    }
}
