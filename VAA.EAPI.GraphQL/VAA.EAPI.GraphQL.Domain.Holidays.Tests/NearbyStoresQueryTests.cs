using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class NearbyStoresQueryTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public NearbyStoresQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();

        _serviceProvider = new ServiceCollection().AddLogging()
                                                     .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                     .AddSingleton<IHttpConnector, HttpConnector>()
                                                     .AddSingleton<IStoreLocatorServiceProvider, StoreServiceProvider>()
                                                     .AddSingleton<NearbyStoresQuery>()
                                                     .BuildServiceProvider();
    }

    [Fact]
    public void Constructor_ShouldThrowNullException_WhenStoreServiceProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new NearbyStoresQuery(null!));
    }

    [Fact]
    public async Task Query_ShouldReturnNearbyStores_ForGoodRequest()
    {
        var _query = _serviceProvider.GetService<NearbyStoresQuery>();
        var result = await _query.GetNearbyStores("SW1A 1AA");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Stores);

        var firstStore = result.Stores.First();
        Assert.Equal("LON001", firstStore.StoreId);
        Assert.Equal("London Heathrow Store", firstStore.Name);
        Assert.True(firstStore.MarkedAsClosed);
        Assert.Equal("Mon", firstStore.StoreWorkHours.First().Day);
        Assert.Equal("09:00", firstStore.StoreWorkHours.First().OpeningTime);
        Assert.False(firstStore.StoreWorkHours.First().IsClosed);
    }
}
