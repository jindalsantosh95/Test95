using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Query.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests
{
    public class StoreLocatorHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public StoreLocatorHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [Constants.Environment.GoogleDistanceMatrixBaseUrl] = "https://maps.googleapis.com/maps/api/distancematrix/json",
                    [Constants.Environment.GoogleDistanceMatrixApiKey] = "test-api-key"
                })
                .Build();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IConfiguration>(configuration)
                                                      .AddSingleton<FakeStoreRepository>()
                                                      .AddSingleton<IStoreRepository>(provider => provider.GetRequiredService<FakeStoreRepository>())
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IGoogleService, GoogleService>()
                                                      .AddSingleton<StoreHandler>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldFail_WhenStoreMissing()
        {
            var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();
            var response = await storeHandler.GetStoreByIdAsync(new StoreSearchRequest { SearchParameter = "SW1A", StoreId = "MISSING" });

            Assert.True(response.IsFailure);
            Assert.Equal("Store with id MISSING was not found.", response.Error);
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldThrowArgumentException_WhenStoreIdMissing()
        {
            var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();
            await Assert.ThrowsAsync<ArgumentException>(() => storeHandler.GetStoreByIdAsync(new StoreSearchRequest { SearchParameter = "SW1A" }));
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldReturnSlots_WhenRequestIsValid()
        {
            var storeRepository = _serviceProvider.GetRequiredService<FakeStoreRepository>();
            var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();
            var selectedDate = new DateTime(2026, 12, 16);

            storeRepository.StoreById["A"] = new StoreResult { StoreId = "A", Name = "Store A", Label = "A", Latitude = 51.5, Longitude = -0.1 };
            storeRepository.SlotsByStoreAndDate[("A", selectedDate.Date)] =
            [
                new() { StartTime = new DateTimeOffset(2026, 12, 16, 9, 0, 0, TimeSpan.Zero), EndTime = new DateTimeOffset(2026, 12, 16, 10, 0, 0, TimeSpan.Zero), IsAvailable = true },
            new() { StartTime = new DateTimeOffset(2026, 12, 16, 10, 0, 0, TimeSpan.Zero), EndTime = new DateTimeOffset(2026, 12, 16, 11, 0, 0, TimeSpan.Zero), IsAvailable = false }
            ];

            var response = await storeHandler.GetStoreSlotsByDateAsync(new StoreSearchRequest { StoreId = "A", SelectedDate = selectedDate });

            Assert.True(response.IsSuccess);
            Assert.Equal("A", response.Value.Value.StoreId);
            Assert.Equal(2, response.Value.Value.Slots.Count());
            Assert.Contains(response.Value.Value.Slots, record => !record.IsAvailable);
        }

        [Fact]
        public async Task GetStoreCalendarAvailabilityAsync_ShouldReturnDayAvailability_WhenRequestIsValid()
        {
            var storeRepository = _serviceProvider.GetRequiredService<FakeStoreRepository>();
            var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();

            storeRepository.StoreById["A"] = new StoreResult { StoreId = "A", Name = "Store A", Label = "A", Latitude = 51.5, Longitude = -0.1 };
            storeRepository.CalendarByStoreAndMonth[("A", 2026, 12)] =
            [
                new() { Date = new DateTime(2026, 12, 16), HasAvailableSlot = false },
            new() { Date = new DateTime(2026, 12, 17), HasAvailableSlot = true }
            ];

            var response = await storeHandler.GetStoreCalendarAvailabilityAsync(new StoreSearchRequest { StoreId = "A", Year = 2026, Month = 12 });

            Assert.True(response.IsSuccess);
            Assert.Equal("A", response.Value.Value.StoreId);
            Assert.Equal(2, response.Value.Value.Days.Count());
            Assert.Contains(response.Value.Value.Days, record => !record.HasAvailableSlot);
        }

        [Fact]
        public async Task GetStoreCalendarAvailabilityAsync_ShouldThrowArgumentException_WhenMonthInvalid()
        {
            var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();
            await Assert.ThrowsAsync<ArgumentException>(() => storeHandler.GetStoreCalendarAvailabilityAsync(new StoreSearchRequest { StoreId = "A", Year = 2026, Month = 13 }));
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldThrowArgumentException_WhenDateMissing()
        {
            var storeHandler = _serviceProvider.GetRequiredService<StoreHandler>();
            await Assert.ThrowsAsync<ArgumentException>(() => storeHandler.GetStoreSlotsByDateAsync(new StoreSearchRequest { StoreId = "A" }));
        }

        private sealed class FakeStoreRepository : IStoreRepository
        {
            public IReadOnlyCollection<StoreResult> Stores { get; set; } = [];
            public Dictionary<string, StoreResult?> StoreById { get; } = new();
            public Dictionary<(string StoreId, DateTime Date), IReadOnlyCollection<StoreSlot>> SlotsByStoreAndDate { get; } = new();
            public Dictionary<(string StoreId, int Year, int Month), IReadOnlyCollection<StoreDayAvailability>> CalendarByStoreAndMonth { get; } = new();
            public Task<IReadOnlyCollection<StoreResult>> GetStoresAsync() => Task.FromResult(Stores);

            public Task<StoreResult?> GetStoreByIdAsync(string storeId)
            {
                StoreById.TryGetValue(storeId, out var store);
                return Task.FromResult(store);
            }

            public Task<IReadOnlyCollection<StoreSlot>> GetStoreSlotsByDateAsync(string storeId, DateTime selectedDate)
            {
                if (SlotsByStoreAndDate.TryGetValue((storeId, selectedDate.Date), out var slots))
                {
                    return Task.FromResult(slots);
                }

                return Task.FromResult<IReadOnlyCollection<StoreSlot>>([]);
            }

            public Task<IReadOnlyCollection<StoreDayAvailability>> GetStoreCalendarAvailabilityAsync(string storeId, int year, int month)
            {
                if (CalendarByStoreAndMonth.TryGetValue((storeId, year, month), out var days))
                {
                    return Task.FromResult(days);
                }

                return Task.FromResult<IReadOnlyCollection<StoreDayAvailability>>([]);
            }
        }
    }
}
