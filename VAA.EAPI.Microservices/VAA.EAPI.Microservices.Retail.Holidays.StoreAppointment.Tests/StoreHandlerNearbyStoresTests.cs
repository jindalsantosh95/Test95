
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Query.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public class StoreHandlerNearbyStoresTests
{
    private readonly IServiceProvider _serviceProvider;

    public StoreHandlerNearbyStoresTests()
    {
        _serviceProvider = new ServiceCollection().AddLogging()
                                                  .AddSingleton<FakeStoreRepository>()
                                                  .AddSingleton<IStoreRepository>(provider => provider.GetRequiredService<FakeStoreRepository>())
                                                  .AddSingleton<FakeGoogleService>()
                                                  .AddSingleton<IGoogleService>(provider => provider.GetRequiredService<FakeGoogleService>())
                                                  .AddSingleton<StoreHandler>()
                                                  .BuildServiceProvider();
    }

    [Fact]
    public async Task GetNearByStoresAsync_ShouldReturnFailure_WhenNoStoresFound()
    {
        var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();

        var response = await storeHandler.GetNearByStoresAsync(new StoreSearchRequest { SearchParameter = "SW1A" });

        Assert.True(response.IsFailure);
        Assert.Equal("No stores found.", response.Error);
    }

    [Fact]
    public async Task GetNearByStoresAsync_ShouldMarkOnlyFirstStoreAsClosest_AndHideOtherFullDetails()
    {
        var storeRepository = _serviceProvider.GetRequiredService<FakeStoreRepository>();
        var googleService = _serviceProvider.GetRequiredService<FakeGoogleService>();
        var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();

        storeRepository.Stores =
        [
            new() { StoreId = "A", Name = "London Bluewater", Label = "Bluewater", FullDetails = new StoreInformation() },
            new() { StoreId = "B", Name = "London Heathrow", Label = "Heathrow", FullDetails = new StoreInformation() }
        ];

        googleService.DistanceByStore = new Dictionary<string, DistanceMatrixResult>
        {
            ["A"] = new DistanceMatrixResult { DistanceInMeters = 50, DistanceText = "50 m", DurationText = "2 mins" },
            ["B"] = new DistanceMatrixResult { DistanceInMeters = 500, DistanceText = "500 m", DurationText = "10 mins" }
        };

        var response = await storeHandler.GetNearByStoresAsync(new StoreSearchRequest { SearchParameter = "bluewater" });

        Assert.True(response.IsSuccess);
        var stores = response.Value.Value.Stores.ToList();
        Assert.Equal("A", stores[0].StoreId);
        Assert.True(stores[0].IsClosest);
        Assert.False(stores[1].IsClosest);
        Assert.NotNull(stores[0].FullDetails);
        Assert.Null(stores[1].FullDetails);
        Assert.Equal("50 m", stores[0].Distance);
        Assert.Equal("2 mins", stores[0].DrivingTime);
    }

    private sealed class FakeStoreRepository : IStoreRepository
    {
        public IReadOnlyCollection<StoreResult> Stores { get; set; } = Array.Empty<StoreResult>();

        public Task<IReadOnlyCollection<StoreResult>> GetStoresAsync() => Task.FromResult(Stores);

        public Task<StoreResult?> GetStoreByIdAsync(string storeId) => Task.FromResult<StoreResult?>(null);

        public Task<IReadOnlyCollection<StoreSlot>> GetStoreSlotsByDateAsync(string storeId, DateTime selectedDate) => Task.FromResult<IReadOnlyCollection<StoreSlot>>(Array.Empty<StoreSlot>());

        public Task<IReadOnlyCollection<StoreDayAvailability>> GetStoreCalendarAvailabilityAsync(string storeId, int year, int month)
            => Task.FromResult<IReadOnlyCollection<StoreDayAvailability>>(Array.Empty<StoreDayAvailability>());
    }

    private sealed class FakeGoogleService : IGoogleService
    {
        public Dictionary<string, DistanceMatrixResult> DistanceByStore { get; set; } = new();

        public Task<IReadOnlyDictionary<string, DistanceMatrixResult>> GetDistanceMetricsAsync(string origin, IReadOnlyCollection<StoreResult> stores)
            => Task.FromResult<IReadOnlyDictionary<string, DistanceMatrixResult>>(DistanceByStore);
    }
}
