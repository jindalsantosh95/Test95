using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class StoreDetailsQueryTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public StoreDetailsQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();

        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                  .AddSingleton<IStoreLocatorServiceProvider, StoreServiceProvider>()
                                                  .AddSingleton<StoreDetailsQuery>()
                                                  .BuildServiceProvider();
    }

    [Fact]
    public void Constructor_ShouldThrowNullException_WhenStoreServiceProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new StoreDetailsQuery(null!));
    }

    [Fact]
    public async Task Query_ShouldReturnStoreDetails_ForGoodRequest()
    {
        var _query = _serviceProvider.GetService<StoreDetailsQuery>();
        var result = await _query.GetStoreDetailsById("LON001", "LON");

        Assert.NotNull(result);
        Assert.Equal("LON001", result.StoreId);
        Assert.Equal("London Heathrow Store", result.Name);
        Assert.True(result.MarkedAsClosed);
        Assert.Equal("0207 000 0000", result.FullDetails?.Details.Phone);
        Assert.Equal("Mon", result.FullDetails?.OpeningHours.WeeklyHours.First().Day);
        Assert.Equal("InStoreExpert", result.FullDetails?.Services.AccessibilityFeatures.First().Name);
        var specialHour = Assert.Single(result.FullDetails?.OpeningHours.SpecialHours!);
        Assert.Equal("25th December", specialHour.Date);
    }
}
