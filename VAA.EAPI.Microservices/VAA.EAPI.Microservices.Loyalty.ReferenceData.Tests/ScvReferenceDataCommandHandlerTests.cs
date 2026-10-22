using Microsoft.Extensions.DependencyInjection;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.PartnerReferenceData;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.ProductReferenceData;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Repository;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Tests
{
    public class ScvReferenceDataCommandHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public ScvReferenceDataCommandHandlerTests()
        {
            ConfigReader.Set("microservicedbkey", "xxx");
            ConfigReader.Set("microserviceeventstoretablename", "test");
            ConfigReader.Set("servicebusconnectionstring", "testConnectionString");
            ConfigReader.Set("servicebustopicname", "testtopic");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<IProductReferenceDataCommandHandler, ProductReferenceDataCommandHandler>()
                .AddSingleton<IProductDataQueryHandler, ProductDataQueryHandler>()
                .AddSingleton<IPartnerReferenceDataCommandHandler, PartnerReferenceDataCommandHandler>()
                .AddSingleton<IPartnerProfileQueryHandler, PartnerProfileQueryHandler>()
                .AddSingleton<SvcReferenceDataController>()
                .AddSingleton<IScvRefDBRepository, ScvRefDBRepository>()
                .AddSingleton<ICacheService, CacheService>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<IScvRefDbService>(_ =>
                {
                    var refDbService = new Mock<IScvRefDbService>();

                    refDbService.Setup(x => x.GetProductsAsync()).ReturnsAsync(new[] { new Product { RowId = "123", ProductName = "Test", ProductType = "Service" } });
                    refDbService.Setup(x => x.GetPartnerCodeAsync()).ReturnsAsync(new[] { new PartnerProfile { RowId = "123", Name = "Test  Partner", PartnerCode = "TP" } });

                    return refDbService.Object;
                })
                .BuildServiceProvider();
        }

        [Fact]
        public async Task ProductDataCommandHandler_ShouldSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IProductReferenceDataCommandHandler>();

            var result = await handler.Handle(new ProductReferenceDataCommand(), CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal("Publish Event Successful", result.Value);
        }


        [Fact]
        public async Task PartnerDataCommandHandler_ShouldSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IPartnerReferenceDataCommandHandler>();

            var result = await handler.Handle(new PartnerReferenceDataCommand(), CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal("Publish Event Successful", result.Value);
        }
    }
}
