using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class StoreManagerQueriesPassThroughTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;

    public StoreManagerQueriesPassThroughTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();
        SetStoreManagerPrincipal();

        _serviceProvider = new ServiceCollection().AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IStoreManagerServiceProvider, StoreServiceProvider>()
            .AddSingleton<GetStoreByIdStoreManagerQuery>()
            .AddSingleton<GetOpeningHoursStoreManagerQuery>()
            .AddSingleton<GetSlotRulesStoreManagerQuery>()
            .AddSingleton<GetSlotAllocationsStoreManagerQuery>()
            .AddSingleton<GetSlotsStoreManagerQuery>()
            .AddSingleton<GetAvailabilityStoreManagerQuery>()
            .AddSingleton<GetAvailabilityRangeStoreManagerQuery>()
            .AddSingleton<GetClosuresStoreManagerQuery>()
            .AddSingleton<GetMeStoreManagerQuery>()
            .AddSingleton<GetBookingsStoreManagerQuery>()
            .AddSingleton<GetMyBookingsStoreManagerQuery>()
            .AddSingleton<GetAdminStoresStoreManagerQuery>()
            .AddSingleton<GetAdminStoreByIdStoreManagerQuery>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task GetStoreById_ShouldReturnStore()
    {
        var result = await GetQuery<GetStoreByIdStoreManagerQuery>().GetStoreById("store-1");

        Assert.Equal("store-1", result.Id);
        Assert.Equal("Bournemouth Next", result.Name);
        Assert.Equal("01202 000000", result.Telephone);
    }

    [Fact]
    public async Task GetOpeningHours_ShouldReturnOpeningHours()
    {
        var result = (await GetQuery<GetOpeningHoursStoreManagerQuery>().GetOpeningHours("store-1")).Single();

        Assert.Equal(1, result.DayOfWeek);
        Assert.Equal("09:00", result.OpensAt);
        Assert.Equal("17:00", result.ClosesAt);
    }

    [Fact]
    public async Task GetSlotRules_ShouldReturnSlotRules()
    {
        var result = (await GetQuery<GetSlotRulesStoreManagerQuery>().GetSlotRules("store-1")).Single();

        Assert.Equal("rule-1", result.Id);
        Assert.Equal(30, result.SlotDurationMinutes);
        Assert.Equal(2, result.SlotCapacity);
    }

    [Fact]
    public async Task GetSlots_ShouldReturnSlots()
    {
        var result = (await GetQuery<GetSlotsStoreManagerQuery>().GetSlots("store-1", "2026-07-01", "2026-07-02")).Single();

        Assert.Equal("slot-1", result.Id);
        Assert.Equal("OPEN", result.Status);
        Assert.Equal(2, result.Capacity);
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T09:00:00Z"), result.StartAt);
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T09:30:00Z"), result.EndAt);
    }

    [Fact]
    public async Task GetSlotAllocations_ShouldReturnAllocations()
    {
        var result = await GetQuery<GetSlotAllocationsStoreManagerQuery>()
            .GetSlotAllocations("store-1", "slot-1", "2026-07-01T09:00:00+01:00");

        Assert.Equal("slot-1", result.SlotId);
        Assert.Equal("store-1", result.StoreId);
        Assert.Equal(3, result.Capacity);
        Assert.Equal(0, result.RemainingCapacity);

        var allocations = result.Allocations.ToArray();
        Assert.Equal(3, allocations.Length);
        Assert.Equal("BLOCKED", allocations[0].Status);
        Assert.Equal("Staff unavailable", allocations[0].Reason);
        Assert.Equal("BOOKED", allocations[1].Status);
        Assert.Equal("BR-10-000171", allocations[1].BookingId);
        Assert.Equal("BR56787", allocations[1].CustomerRef);
    }

    [Fact]
    public async Task GetAvailability_ShouldReturnAvailability()
    {
        var result = (await GetQuery<GetAvailabilityStoreManagerQuery>().GetAvailability("store-1", "2026-07-01")).Single();

        Assert.Equal("availability-1", result.Id);
        Assert.Equal(1, result.RemainingCapacity);
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T09:00:00Z"), result.StartAt);
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T09:30:00Z"), result.EndAt);
    }

    [Fact]
    public async Task GetAvailabilityRange_ShouldReturnAvailability()
    {
        var result = (await GetQuery<GetAvailabilityRangeStoreManagerQuery>().GetAvailabilityRange("store-1", "2026-07-01", "2026-07-02")).Single();

        Assert.Equal("availability-range-1", result.Id);
        Assert.Equal("store-1", result.StoreId);
    }

    [Fact]
    public async Task GetClosures_ShouldReturnClosures()
    {
        var result = (await GetQuery<GetClosuresStoreManagerQuery>().GetClosures("store-1", "2026-07-01", "2026-07-31")).Single();

        Assert.Equal("closure-1", result.Id);
        Assert.Equal(new NodaTime.LocalDate(2026, 7, 15), result.ClosureDate);
        Assert.Equal("FULL_DAY", result.ClosureType);
    }

    [Fact]
    public async Task GetMe_ShouldReturnAuthenticatedStoreManager()
    {
        var result = await GetQuery<GetMeStoreManagerQuery>().GetMe();

        Assert.Equal("user-1", result.UserId);
        Assert.Equal("manager@example.com", result.Username);
        Assert.Equal("store-1", result.StoreId);
        Assert.Equal(Roles.StoreManager, result.Role);
    }

    [Fact]
    public async Task GetMyBookings_ShouldReturnBookings()
    {
        var result = (await GetQuery<GetMyBookingsStoreManagerQuery>()
            .GetMyBookings("2026-07-01", "BOOKED", "ABC123")).Single();

        Assert.Equal("booking-1", result.Id);
        Assert.Equal("slot-1", result.StoreSlotId);
        Assert.Equal("ABC123", result.CustomerRef);
        Assert.Equal("BOOKED", result.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T09:00:00Z"), result.WindowStartAt);
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T09:30:00Z"), result.WindowEndAt);
        Assert.Equal(DateTimeOffset.Parse("2026-06-01T10:00:00Z"), result.CreatedAt);
        Assert.Equal(DateTimeOffset.Parse("2026-06-01T10:00:00Z"), result.UpdatedAt);
    }

    [Fact]
    public async Task GetBookings_ShouldReturnCancelledHistoryForSelectedStore()
    {
        var result = (await GetQuery<GetBookingsStoreManagerQuery>()
            .GetBookings("store-2", "2026-07-29", StoreManagerBookingStatus.Cancelled))
            .Single();

        Assert.Equal("booking-cancelled", result.Id);
        Assert.Equal("store-2", result.StoreId);
        Assert.Equal("slot-1", result.StoreSlotId);
        Assert.Equal("CANCELLED", result.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-07-22T11:30:00Z"), result.UpdatedAt);
    }

    [Fact]
    public async Task GetAdminStores_ShouldReturnStores()
    {
        var result = (await GetQuery<GetAdminStoresStoreManagerQuery>().GetAdminStores()).Single();

        Assert.Equal("admin-store-1", result.Id);
        Assert.Equal("Admin Store", result.Name);
    }

    [Fact]
    public async Task GetAdminStoreById_ShouldReturnStore()
    {
        var result = await GetQuery<GetAdminStoreByIdStoreManagerQuery>().GetAdminStoreById("admin-store-1");

        Assert.Equal("admin-store-1", result.Id);
        Assert.Equal("Admin Store", result.Name);
    }

    private T GetQuery<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();

    private static void SetStoreManagerPrincipal()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Name, "manager@example.com"),
            new Claim("storeId", "store-1"),
            new Claim(ClaimTypes.Role, Roles.StoreManager)
        ], "Test");

        AsyncContext.Current.Context.Features.Set(
            new JwtPrincipalFeature(new ClaimsPrincipal(identity), new AuthToken { Token = "test-token" }));
    }
}
