using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.ValidateUniqueId;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class ValidateUniqueIdHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        public ValidateUniqueIdHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {

            ConfigReader.Set("microservicedbkey", "testDBKey");

            _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
            .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
            .AddSingleton<IValidateUniqueIdRepository, ValidateUniqueIdRepository>()
            .AddTransient<IValidateUniqueIdHandler, ValidateUniqueIdHandler>()
            .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ShouldReturnValid_WhenUniqueIdIsValid()
        {
            var handler = _serviceProvider.GetRequiredService<IValidateUniqueIdHandler>();
            var tableService = _serviceProvider.GetRequiredService<ITableStorageService<CampaignUniqueIdTableEntity>>();
            await tableService.InsertOrMergeAsync(new CampaignUniqueIdTableEntity
            {
                PartitionKey = "PCM",
                RowKey = "PCM-UID-1001",
                UniqueId = "PCM-UID-1001",
                FCNumber = null,
                UsedTimestamp = null
            });
            var result = await handler.Handle(new ValidateUniqueIdCommand { UniqueId = "PCM-UID-1001" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.True(result.Value.Value.Valid);
            Assert.False(result.Value.Value.Claimed);
            Assert.Null(result.Value.Value.FcNumber);
            Assert.Null(result.Value.Value.ClaimedDateTime);
        }

        [Fact]
        public async Task Handle_ShouldReturnClaimed_WhenUniqueIdAlreadyClaimed()
        {
            var handler = _serviceProvider.GetRequiredService<IValidateUniqueIdHandler>();
            var tableService = _serviceProvider.GetRequiredService<ITableStorageService<CampaignUniqueIdTableEntity>>();

            await tableService.InsertOrMergeAsync(new CampaignUniqueIdTableEntity
            {
                PartitionKey = "PCM",
                RowKey = "PCM-UID-2001",
                UniqueId = "PCM-UID-2001",
                FCNumber = "21123523464",
                UsedTimestamp = "2026-03-10T10:00:00Z"
            });
            var result = await handler.Handle(new ValidateUniqueIdCommand { UniqueId = "PCM-UID-2001" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.True(result.Value.Value.Valid);
            Assert.True(result.Value.Value.Claimed);
            Assert.Equal("21123523464", result.Value.Value.FcNumber);
            Assert.Equal(DateTimeOffset.Parse("2026-03-10T10:00:00Z").UtcDateTime, result.Value.Value.ClaimedDateTime);
        }

        [Fact]
        public async Task Handle_ShouldReturnBusinessException_WhenUniqueIdNotFound()
        {
            var handler = _serviceProvider.GetRequiredService<IValidateUniqueIdHandler>();
            var result = await handler.Handle(new ValidateUniqueIdCommand { UniqueId = "PCM-UID-NOT-FOUND" }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.NotEmpty(result.Value.Errors);
            Assert.Equal("10014", result.Value.Errors.First().Code);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnBadRequest_WhenUniqueIdIsMissing()
        {
            var handler = _serviceProvider.GetRequiredService<IValidateUniqueIdHandler>();
            var result = await handler.Handle(new ValidateUniqueIdCommand { UniqueId = string.Empty }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.NotEmpty(result.Value.Errors);
            Assert.Null(result.Value.Value);
        }
    }
}
