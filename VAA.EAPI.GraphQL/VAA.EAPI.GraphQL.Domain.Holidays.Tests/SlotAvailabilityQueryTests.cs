using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class SlotAvailabilityQueryTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public SlotAvailabilityQueryTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();

        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                  .AddSingleton<IHttpConnector, HttpConnector>()
                                                  .AddSingleton<IStoreLocatorServiceProvider, StoreServiceProvider>()
                                                  .AddSingleton<SlotAvailabilityQuery>()
                                                  .BuildServiceProvider();
    }

    [Fact]
    public void Constructor_ShouldThrowNullException_WhenStoreServiceProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SlotAvailabilityQuery(null!));
    }

    [Fact]
    public async Task Query_ShouldReturnSlots_ForGoodRequest()
    {
        var _query = _serviceProvider.GetService<SlotAvailabilityQuery>();
        var result = await _query.GetStoreSlotsByDate("LON001", new DateTime(2026, 7, 16));

        Assert.NotNull(result);
        Assert.Equal("LON001", result.StoreId);
        Assert.Equal(2, result.Slots.Count());
        Assert.True(result.Slots.First().IsAvailable);
        Assert.Equal("2026-07-16T09:00:00+01:00", result.Slots.First().StartTime.ToString("yyyy-MM-ddTHH:mm:sszzz"));
    }
}
