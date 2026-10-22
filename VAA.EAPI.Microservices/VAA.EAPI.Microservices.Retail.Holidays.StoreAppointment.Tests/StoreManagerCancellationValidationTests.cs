using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public sealed class StoreManagerCancellationValidationTests
{
    private readonly IServiceProvider _serviceProvider;

    public StoreManagerCancellationValidationTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddDbContext<StoreAppointmentDbContext>(
                options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()))
            .AddScoped<StoreManagerRepository>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task CancelBooking_ShouldRejectMissingCustomerContactConfirmation()
    {
        var result = await CancelBooking(new BookingCancellationRequest
        {
            Reason = "STORE_CLOSURE",
            CustomerContactConfirmed = false,
            CancelledByUserId = "manager-1"
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("CUSTOMER_CONTACT_NOT_CONFIRMED", result.ErrorCode);
    }

    [Fact]
    public async Task CancelBooking_ShouldRejectUnsupportedReason()
    {
        var result = await CancelBooking(new BookingCancellationRequest
        {
            Reason = "CUSTOMER_REQUEST",
            CustomerContactConfirmed = true,
            CancelledByUserId = "manager-1"
        });

        Assert.True(result.IsConflict);
        Assert.Equal("BOOKING_NOT_CANCELLABLE", result.ErrorCode);
    }

    [Fact]
    public async Task CancelBooking_ShouldRequireCancellationActorUserId()
    {
        var result = await CancelBooking(new BookingCancellationRequest
        {
            Reason = "STORE_CLOSURE",
            CustomerContactConfirmed = true,
            CancelledByUserId = string.Empty
        });

        Assert.True(result.IsValidationError);
        Assert.Equal("CANCELLATION_ACTOR_REQUIRED", result.ErrorCode);
        Assert.Equal("Cancellation actor user id is required.", result.ErrorMessage);
    }

    private Task<ServiceResult<ManageBookingResponse>> CancelBooking(BookingCancellationRequest request) =>
        _serviceProvider.GetRequiredService<StoreManagerRepository>()
            .CancelBooking("store-1", "booking-1", request);
}
