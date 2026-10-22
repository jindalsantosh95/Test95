using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.Content.Application;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Content.Tests
{
    public class RefreshSalesBannersCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public RefreshSalesBannersCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("AemGraphEndpointUri", "https://test1-vaahome.webdev.vholsinternal.co.uk/graphql/execute.json/vaa-home/");
            ConfigReader.Set("AemImagePrefix", "https://stg-vaabrowse.virginatlantic.com/");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IDistributedCache>(DistributedCache.Instance)
                                                      .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                                                      .AddSingleton<IContentRequestHandler, ContentRequestHandler>()
                                                      .AddSingleton<IRefreshSalesBannersCommandHandler, RefreshSalesBannersCommandHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_Should_Success()
        {
            /* Arrange */
            var uut = _serviceProvider.GetService<IRefreshSalesBannersCommandHandler>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();

            /* Act */
            await uut.Handle(CancellationToken.None);

            /* Assert */
            var cachedSalesBanners = await distributedCacheService.Get<string>("sales-banners:search-top");
            Assert.NotNull(cachedSalesBanners);

            // TODO (Maybe) : Add more asserts to validate the content of the cached sales banners
        }
    }
}
