using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Attributes;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.CRM.Content.Application;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Content.Tests
{
    [TestCaseOrderer(typeof(VAA.EAPI.Core.Xunit.TestCaseRunOrder))]
    public class SalesBannersCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>, IClassFixture<InMemoryFixture>
    {
        private readonly InMemoryFixture _inMemoryFixture;
        private readonly IServiceProvider _serviceProvider;

        public SalesBannersCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture, InMemoryFixture inMemoryFixture)
        {
            _inMemoryFixture = inMemoryFixture;

            ConfigReader.Set("AemGraphEndpointUri", "https://test1-vaahome.webdev.vholsinternal.co.uk/graphql/execute.json/vaa-home/");
            ConfigReader.Set("AemImagePrefix", "https://stg-vaabrowse.virginatlantic.com/");

            _serviceProvider = _inMemoryFixture.TryGet<IServiceProvider>("_inMemoryFixture");

            _serviceProvider ??= new ServiceCollection().AddLogging()
                                                        .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                        .AddSingleton<IHttpConnector, HttpConnector>()
                                                        .AddSingleton<IDistributedCache>(DistributedCache.Instance)
                                                        .AddSingleton<IDistributedCacheService, DistributedCacheService>()
                                                        .AddSingleton<IAzureBlobService, Core.Xunit.Azure.AzureBlobService>()
                                                        .AddSingleton<IContentRequestHandler, ContentRequestHandler>()
                                                        .AddSingleton<IRefreshSalesBannersCommandHandler, RefreshSalesBannersCommandHandler>()
                                                        .AddSingleton<ISalesBannersCommandHandler, SalesBannersCommandHandler>()

                                                        .BuildServiceProvider();

            _inMemoryFixture.TryAdd("_serviceProvider", _serviceProvider);
        }

        [Fact]
        [Order(0)]
        public async Task LoadCacheWithSalesBannerData()
        {
            // We need to load the cache with sales banner data before running other tests


            /* Arrange */
            var refreshCache = _serviceProvider.GetService<IRefreshSalesBannersCommandHandler>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();

            /* Act */
            await refreshCache.Handle(CancellationToken.None);

            /* Assert */
            // Just check that something is in the cache
            var cachedSalesBanners = await distributedCacheService.Get<string>("sales-banners:search-top");

            Assert.NotNull(cachedSalesBanners);
        }

        [Fact]
        [Order(1)]
        public async Task MultipleDisplayLocations_BothMatchAndCorrectlyOrdered()
        {
            //await LoadCacheWithSalesBannerData();

            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top", "seasonal-messaging" },
                    Request = new()
                    {
                        BookingType = "flight-only",
                        OriginAirport = "LHR"
                    }
                }
            };

            var cache = _serviceProvider.GetService<IDistributedCacheService>();
            var blob = _serviceProvider.GetService<IAzureBlobService>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();
            var uat = _serviceProvider.GetService<ISalesBannersCommandHandler>();

            // Act
            var result = await uat.Handle(cmd, CancellationToken.None);

            // Assert - We have a match for each display location, containing the matching test sales banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(1, result["search-top"]?.Count());
            Assert.True(result.ContainsKey("seasonal-messaging"));
            Assert.Equal(4, result["seasonal-messaging"]?.Count());
            Assert.Equal(2, result.Count);

            // Banners are ordered in increasing order of Ranking
            Assert.Equal(result["search-top"].Min(b => b.Ranking), result["search-top"].First().Ranking);
            Assert.Equal(result["search-top"].Max(b => b.Ranking), result["search-top"].Last().Ranking);
        }

        [Fact]
        [Order(3)]
        public async Task SalesBanner_NoMatch()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        OriginAirport = "LGW",
                        DestinationAirport = "LHR"
                    }
                }
            };

            var cache = _serviceProvider.GetService<IDistributedCacheService>();
            var blob = _serviceProvider.GetService<IAzureBlobService>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();
            var uat = _serviceProvider.GetService<ISalesBannersCommandHandler>();

            // Act
            var result = await uat.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing empty list of banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(0, result["search-top"]?.Count());
            Assert.Single(result);
        }

        [Fact]
        [Order(4)]
        public async Task SalesBanner_DepartureAirportOnly()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        OriginAirport = "LHR",
                    }
                }
            };

            var cache = _serviceProvider.GetService<IDistributedCacheService>();
            var blob = _serviceProvider.GetService<IAzureBlobService>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();
            var uat = _serviceProvider.GetService<ISalesBannersCommandHandler>();

            // Act
            var result = await uat.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing all matching sales banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(1, result["search-top"]?.Count());
            Assert.Single(result);

            // Banners are ordered in increasing order of Ranking
            Assert.Equal("Start your journey in style at the Virgin Atlantic Clubhouse", result["search-top"].First().Title);
        }

        [Fact]
        [Order(5)]
        public async Task SalesBanner_DestinationAirport()
        {
            //await LoadCacheWithSalesBannerData();

            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "search-top" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        OriginAirport = "MAN",
                        DestinationAirport = "LAS"
                    }
                }
            };

            var cache = _serviceProvider.GetService<IDistributedCacheService>();
            var blob = _serviceProvider.GetService<IAzureBlobService>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();
            var uat = _serviceProvider.GetService<ISalesBannersCommandHandler>();

            // Act
            var result = await uat.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing all matching sales banners. No other locations returned.
            Assert.True(result.ContainsKey("search-top"));
            Assert.Equal(2, result["search-top"]?.Count());
            Assert.Single(result);

            // Banners are ordered in increasing order of Ranking
            Assert.Equal("Did you know?", result["search-top"].First().Title);
            Assert.Equal("Discover Las Vegas", result["search-top"].Last().Title);
        }

        [Fact]
        [Order(6)]
        public async Task SeasonalMessaging_BookingType_NullRanking()
        {
            // Arrange
            var cmd = new SalesBannersCommand
            {
                Filters = new Domain.SalesBanner.SalesBannerFilters
                {
                    DisplayLocations = new string[] { "seasonal-messaging" },
                    Request = new Domain.SalesBanner.SalesBannerRequest
                    {
                        BookingType = "flight-only"
                    }
                }
            };

            var cache = _serviceProvider.GetService<IDistributedCacheService>();
            var blob = _serviceProvider.GetService<IAzureBlobService>();
            var distributedCacheService = _serviceProvider.GetService<IDistributedCacheService>();
            var uat = _serviceProvider.GetService<ISalesBannersCommandHandler>();

            // Act
            var result = await uat.Handle(cmd, CancellationToken.None);

            // Assert

            // We have a match for the searched display location, containing all matching flight-only seasonal messages. No other locations returned.
            Assert.True(result.ContainsKey("seasonal-messaging"));
            Assert.Equal(3, result["seasonal-messaging"]?.Count());
            Assert.Single(result);

            // Banners are ordered in increasing order of Ranking, with null first
            Assert.Equal("No Route Null", result["seasonal-messaging"].ElementAt(0).Title);
            Assert.Equal("No Route Seven", result["seasonal-messaging"].ElementAt(1).Title);
            Assert.Equal("No Route Twelve", result["seasonal-messaging"].ElementAt(2).Title);
        }
    }
}
