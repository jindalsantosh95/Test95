using System.Security.Claims;
using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NodaTime;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using VAA.EAPI.GraphQL.Domain.Holidays.Mutations;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class StoreManagerMutationsPassThroughTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly HttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;

    public StoreManagerMutationsPassThroughTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        Setup.CreateTestEnvironmentSettings();
        SetStoreManagerPrincipal();
        _httpClientFactory = httpClientFactoryFixture.Current;

        _serviceProvider = new ServiceCollection().AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IStoreManagerServiceProvider, StoreServiceProvider>()
            .AddSingleton<UpdateStoreMutation>()
            .AddSingleton<ReplaceOpeningHoursMutation>()
            .AddSingleton<UpdateSlotStatusMutation>()
            .AddSingleton<UpdateSlotAllocationStatusMutation>()
            .AddSingleton<CreateClosureMutation>()
            .AddSingleton<CancelBookingMutation>()
            .AddSingleton<DeleteClosureMutation>()
            .AddSingleton<UpdateAdminStoreMutation>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnUpdatedStore()
    {
        var result = await GetMutation<UpdateStoreMutation>().UpdateStore("store-1", BuildStoreRequest());

        Assert.Equal("store-1", result.Id);
        Assert.Equal("Updated Store", result.Name);
        Assert.Equal("01202 111111", result.Telephone);
    }

    [Fact]
    public async Task ReplaceOpeningHours_ShouldReturnUpdatedOpeningHours()
    {
        var request = new OpeningHoursReplaceRequest
        {
            OpeningHours =
            [
                new OpeningHoursEntryRequest { DayOfWeek = 1, OpensAt = "08:00", ClosesAt = "18:00" }
            ]
        };

        var result = (await GetMutation<ReplaceOpeningHoursMutation>()
            .ReplaceOpeningHours("store-1", request)).Single();

        Assert.Equal(1, result.DayOfWeek);
        Assert.Equal("08:00", result.OpensAt);
        Assert.Equal("18:00", result.ClosesAt);
    }

    [Fact]
    public async Task UpdateSlotStatus_ShouldReturnUpdatedSlot()
    {
        var result = await GetMutation<UpdateSlotStatusMutation>().UpdateSlotStatus(
            "store-1",
            "slot-1",
            new SlotStatusUpdateRequest
            {
                Status = "CLOSED",
                Reason = "Training",
                StartAt = DateTimeOffset.Parse("2026-07-01T09:00:00+01:00"),
                EndAt = DateTimeOffset.Parse("2026-07-01T09:30:00+01:00")
            });

        Assert.Equal("slot-1", result.Id);
        Assert.Equal("CLOSED", result.Status);
        Assert.Equal("Training", result.Reason);
    }

    [Fact]
    public async Task UpdateSlotAllocationStatus_ShouldReturnUpdatedAllocations()
    {
        var result = await GetMutation<UpdateSlotAllocationStatusMutation>()
            .UpdateSlotAllocationStatus(
                "store-1",
                "slot-1",
                "allocation-20260701-1",
                new StoreManagerSlotAllocationStatusUpdateRequest
                {
                    Status = "BLOCKED",
                    Reason = "Staff unavailable"
                });

        Assert.Equal("slot-1", result.SlotId);
        Assert.Equal(0, result.RemainingCapacity);

        var allocation = Assert.Single(result.Allocations, x => x.Id == "allocation-20260701-1");
        Assert.Equal("BLOCKED", allocation.Status);
        Assert.Equal("Staff unavailable", allocation.Reason);
    }

    [Fact]
    public async Task CreateClosure_ShouldReturnCreatedClosure()
    {
        var result = await GetMutation<CreateClosureMutation>().CreateClosure(
            "store-1",
            new StoreClosureRequest
            {
                ClosureDate = new LocalDate(2026, 12, 25),
                ClosureType = "FULL_DAY",
                Reason = "Christmas"
            });

        Assert.Equal("closure-created-1", result.Id);
        Assert.Equal(new LocalDate(2026, 12, 25), result.ClosureDate);
        Assert.Equal("Christmas", result.Reason);
    }

    [Fact]
    public async Task CancelBooking_ShouldReturnCancelledBooking()
    {
        var result = await GetMutation<CancelBookingMutation>().CancelBooking(
            "store-1",
            "BR-10-000171",
            new BookingCancellationRequest
            {
                Reason = BookingCancellationReason.StoreClosure,
                CustomerContactConfirmed = true
            });

        Assert.Equal("BR-10-000171", result.Id);
        Assert.Equal("store-1", result.StoreId);
        Assert.Equal("slot-1", result.StoreSlotId);
        Assert.Equal("CANCELLED", result.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-07-22T11:30:00Z"), result.UpdatedAt);

        var downstreamResponse = _httpClientFactory
            .GetResponses("https://Testresources.virginatlantic.com")
            .Last(response => response.RequestMessage?.RequestUri?.AbsolutePath.EndsWith(
                "/stores/store-1/bookings/BR-10-000171/cancel",
                StringComparison.Ordinal) == true);
        var requestBody = JObject.Parse(
            await downstreamResponse.RequestMessage!.Content!.ReadAsStringAsync());

        Assert.Equal(
            "STORE_CLOSURE",
            requestBody.GetValue("reason", StringComparison.OrdinalIgnoreCase)?.Value<string>());
        Assert.True(
            requestBody.GetValue("customerContactConfirmed", StringComparison.OrdinalIgnoreCase)?.Value<bool>());
        Assert.Equal(
            "user-1",
            requestBody.GetValue("cancelledByUserId", StringComparison.OrdinalIgnoreCase)?.Value<string>());
        Assert.Equal(
            "manager@example.com",
            requestBody.GetValue("cancelledByDisplayName", StringComparison.OrdinalIgnoreCase)?.Value<string>());
    }

    [Fact]
    public async Task CancelBooking_ShouldRejectMissingCustomerContactConfirmation()
    {
        var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
            GetMutation<CancelBookingMutation>().CancelBooking(
                "store-1",
                "BR-10-000171",
                new BookingCancellationRequest
                {
                    Reason = BookingCancellationReason.StoreClosure,
                    CustomerContactConfirmed = false
                }));

        Assert.Equal("CUSTOMER_CONTACT_NOT_CONFIRMED", Assert.Single(exception.Errors).Code);
    }

    [Fact]
    public async Task DeleteClosure_ShouldReturnTrue()
    {
        var result = await GetMutation<DeleteClosureMutation>().DeleteClosure("store-1", "closure-1");

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAdminStore_ShouldReturnUpdatedStore()
    {
        var result = await GetMutation<UpdateAdminStoreMutation>()
            .UpdateAdminStore("admin-store-1", BuildStoreRequest());

        Assert.Equal("admin-store-1", result.Id);
        Assert.Equal("Updated Admin Store", result.Name);
    }

    private T GetMutation<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();

    private static StoreRequest BuildStoreRequest() =>
        new()
        {
            Name = "Updated Store",
            AddressLine1 = "1 Test Street",
            CityTown = "Bournemouth",
            PostalCode = "BH1 1AA",
            Telephone = "01202 111111"
        };

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
