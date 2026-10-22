using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class PartnerDataServiceTests
    {
        private readonly IServiceProvider _serviceProvider;

        public PartnerDataServiceTests()
        {
            ConfigReader.Set(Constants.Environment.ReferenceMicroServiceDBKey, "test-storage-connection-string");
            ConfigReader.Set(Constants.Environment.SkyTeamPartnerCacheExpiryHours, "1");

            _serviceProvider = new ServiceCollection()
                .AddMemoryCache()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IPartnerDataService, PartnerDataService>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task GetSkyTeamPartnersAsync_ReturnsOnlySkyTeamPartnersInPartnerPartition()
        {
            var tableStorage = _serviceProvider.GetRequiredService<ITableStorageService<PartnerDetails>>();
            await InsertPartnersAsync(tableStorage,
            [
                new PartnerDetails
                {
                    PartnerCode = "KQ",
                    SkyTeamPartnerFlag = true,
                    PartitionKey = Constants.Application.PartnerPartitionKey,
                    RowKey = "KQ"
                },
                new PartnerDetails
                {
                    PartnerCode = "BA",
                    SkyTeamPartnerFlag = false,
                    PartitionKey = Constants.Application.PartnerPartitionKey,
                    RowKey = "BA"
                },
                new PartnerDetails
                {
                    PartnerCode = "AF",
                    SkyTeamPartnerFlag = true,
                    PartitionKey = "Other",
                    RowKey = "AF"
                }
            ]);
            var service = _serviceProvider.GetRequiredService<IPartnerDataService>();

            var partners = await service.GetSkyTeamPartnersAsync(CancellationToken.None);

            var partner = Assert.Single(partners);
            Assert.Equal("KQ", partner.PartnerCode);
        }

        [Fact]
        public async Task GetSkyTeamPartnersAsync_ThrowsWhenCancelled()
        {
            var service = _serviceProvider.GetRequiredService<IPartnerDataService>();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => service.GetSkyTeamPartnersAsync(cancellationTokenSource.Token));
        }

        [Fact]
        public async Task GetSkyTeamPartnersAsync_UsesCachedPartnersUntilTheConfiguredExpiry()
        {
            var tableStorage = _serviceProvider.GetRequiredService<ITableStorageService<PartnerDetails>>();
            await InsertPartnersAsync(tableStorage,
            [
                new PartnerDetails
                {
                    PartnerCode = "KQ",
                    SkyTeamPartnerFlag = true,
                    PartitionKey = Constants.Application.PartnerPartitionKey,
                    RowKey = "KQ"
                }
            ]);
            var service = _serviceProvider.GetRequiredService<IPartnerDataService>();

            var firstRead = await service.GetSkyTeamPartnersAsync(CancellationToken.None);
            await tableStorage.DeleteAsync(new PartnerDetails { RowKey = "KQ" });
            var cachedRead = await service.GetSkyTeamPartnersAsync(CancellationToken.None);

            Assert.Equal(firstRead, cachedRead);
            Assert.Single(cachedRead);
        }

        private static async Task InsertPartnersAsync(ITableStorageService<PartnerDetails> tableStorage, IEnumerable<PartnerDetails> partners)
        {
            foreach (var partner in partners)
            {
                await tableStorage.InsertOrMergeAsync(partner);
            }
        }
    }
}
