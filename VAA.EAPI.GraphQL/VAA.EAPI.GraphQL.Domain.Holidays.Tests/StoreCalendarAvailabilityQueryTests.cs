using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class StoreCalendarAvailabilityQueryTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public StoreCalendarAvailabilityQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();

        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                  .AddSingleton<IStoreLocatorServiceProvider, StoreServiceProvider>()
                                                  .AddSingleton<StoreCalendarAvailabilityQuery>()
                                                  .BuildServiceProvider();
    }

    [Fact]
    public void Constructor_ShouldThrowNullException_WhenStoreServiceProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new StoreCalendarAvailabilityQuery(null!));
    }

    [Fact]
    public async Task Query_ShouldReturnCalendarAvailability_ForGoodRequest()
    {
        var _query = _serviceProvider.GetService<StoreCalendarAvailabilityQuery>();
        var result = await _query.GetStoreCalendarAvailability("LON001", 2026, 7);

        Assert.NotNull(result);
        Assert.Equal("LON001", result.StoreId);
        Assert.Equal(2026, result.Year);
        Assert.Equal(7, result.Month);
        Assert.Equal(2, result.Days.Count());
        Assert.True(result.Days.First().HasAvailableSlot);
    }
}
