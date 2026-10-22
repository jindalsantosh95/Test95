using Microsoft.Extensions.DependencyInjection;
using Moq;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Repository;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Tests
{
    public class ScvReferenceDataQueryHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public ScvReferenceDataQueryHandlerTests()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddSingleton<IProductDataQueryHandler, ProductDataQueryHandler>()
                .AddSingleton<IPartnerProfileQueryHandler, PartnerProfileQueryHandler>()
                .AddSingleton<SvcReferenceDataController>()
                .AddSingleton<IScvRefDBRepository, ScvRefDBRepository>()
                .AddSingleton<ICacheService, CacheService>()
                .AddSingleton<IScvRefDbService>(_ =>
                {
                    var refDbService = new Mock<IScvRefDbService>();

                    refDbService.Setup(x => x.GetProductsAsync()).ReturnsAsync(new[] { new Product { RowId = "123", ProductName = "Test", ProductType = "Service" } });
                    refDbService.Setup(x => x.GetPartnerCodeAsync()).ReturnsAsync(new[] { new PartnerProfile { RowId = "123", Name = "Test  Partner", PartnerCode = "TP", SkyTeamPartnerFlag = 1 } });

                    return refDbService.Object;
                })
                .BuildServiceProvider();
        }

        [Fact]
        public async Task ProductDataQueryHandler_ShouldSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<Application.Interface.IProductDataQueryHandler>();

            var result = await handler.Handle(new ProductDataQuery(), CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal("Test", result.Value.Value.First().ProductName);
        }

        [Fact]
        public async Task PartnerProfileDataQueryHandler_ShouldSuccess()
        {
            var handler = _serviceProvider.GetRequiredService<IPartnerProfileQueryHandler>();

            var result = await handler.Handle(new PartnerProfileQuery(), CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Equal("TP", result.Value.Value.First().PartnerCode);
            Assert.Equal(1, result.Value.Value.First().SkyTeamPartnerFlag);
        }
    }
}
