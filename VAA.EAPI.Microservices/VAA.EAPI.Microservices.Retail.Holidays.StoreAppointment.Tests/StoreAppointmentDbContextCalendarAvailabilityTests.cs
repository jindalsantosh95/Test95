using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public class StoreAppointmentDbContextCalendarAvailabilityTests
{
    [Fact]
    public async Task CalendarAvailability_ShouldReturnOnlyPresentAndFutureDates_WhenMonthIncludesPastDates()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Thu"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Thu", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.DoesNotContain(results, day => day.Date < new DateOnly(2026, 7, 2));
        Assert.Contains(results, day => day.Date == new DateOnly(2026, 7, 2));
    }

    [Fact]
    public async Task CalendarAvailability_ShouldReturnUnavailable_WhenOverlappingAppointmentsReachCapacity()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        dbContext.StoreAppointments.Add(new Appointment
        {
            Id = "appointment-1",
            LocationId = "store-1",
            Date = new DateOnly(2026, 7, 3),
            SlotStart = new TimeOnly(9, 30),
            SlotEnd = new TimeOnly(9, 45)
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.False(results.Single(day => day.Date == new DateOnly(2026, 7, 3)).HasAvailableSlot);
    }

    [Fact]
    public async Task CalendarAvailability_ShouldReturnAvailable_WhenOverlappingAppointmentIsCancelled()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        dbContext.StoreAppointments.Add(new Appointment
        {
            Id = "appointment-1",
            LocationId = "store-1",
            Date = new DateOnly(2026, 7, 3),
            SlotStart = new TimeOnly(9, 30),
            SlotEnd = new TimeOnly(9, 45),
            AppointmentStatus = Constants.Constants.CancelledAppointmentStatus
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.True(results.Single(day => day.Date == new DateOnly(2026, 7, 3)).HasAvailableSlot);
    }

    [Fact]
    public async Task CalendarAvailability_ShouldReturnUnavailable_WhenSlotHasStoreManagerBlock()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0), capacity: 2));
        dbContext.StoreAppointments.Add(new Appointment
        {
            Id = "block-1",
            LocationId = "store-1",
            Date = new DateOnly(2026, 7, 3),
            SlotStart = new TimeOnly(9, 0),
            SlotEnd = new TimeOnly(10, 0),
            VisitPurpose = Constants.Constants.StoreManagerVisitPurpose,
            VisitPurposeDetails = Constants.Constants.StoreManagerBlockedVisitPurposeDetails
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.False(results.Single(day => day.Date == new DateOnly(2026, 7, 3)).HasAvailableSlot);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"details\"")]
    [InlineData("123")]
    public async Task CalendarAvailability_ShouldTreatNonObjectAdditionalDetailsAsFullSlotBlock(string additionalDetails)
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0), capacity: 2));
        dbContext.StoreAppointments.Add(new Appointment
        {
            Id = "block-1",
            LocationId = "store-1",
            Date = new DateOnly(2026, 7, 3),
            SlotStart = new TimeOnly(9, 0),
            SlotEnd = new TimeOnly(10, 0),
            VisitPurpose = Constants.Constants.StoreManagerVisitPurpose,
            VisitPurposeDetails = Constants.Constants.StoreManagerBlockedVisitPurposeDetails,
            AdditionalDetails = additionalDetails
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.False(results.Single(day => day.Date == new DateOnly(2026, 7, 3)).HasAvailableSlot);
    }

    [Fact]
    public async Task CalendarAvailability_ShouldReturnEmpty_WhenRequestedMonthIsBeforeToday()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Mon"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Mon", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 6);

        Assert.Empty(results);
    }

    [Fact]
    public async Task CalendarAvailability_ShouldReturnAvailable_WhenSlotIsInsideOpeningHours()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.True(results.Single(day => day.Date == new DateOnly(2026, 7, 3)).HasAvailableSlot);
    }

    [Fact]
    public async Task CalendarAvailability_ShouldReturnUnavailable_WhenStoreIsMarkedClosed()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        var store = BuildStore("store-1", "Fri");
        store.MarkedAsClosed = true;
        dbContext.Stores.Add(store);
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.False(results.Single(day => day.Date == new DateOnly(2026, 7, 3)).HasAvailableSlot);
    }

    [Fact]
    public async Task CalendarAvailability_ShouldReturnUnavailable_WhenSlotIsOutsideOpeningHours()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri", "12:00", "16:00"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetCalendarAvailabilityAsync(dbContext, "store-1", 2026, 7);

        Assert.False(results.Single(day => day.Date == new DateOnly(2026, 7, 3)).HasAvailableSlot);
    }

    [Fact]
    public async Task StoreSlotsByDate_ShouldReturnUnavailable_WhenSlotHasStoreManagerBlockWithRemainingCapacity()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0), capacity: 2));
        dbContext.StoreAppointments.Add(new Appointment
        {
            Id = "block-1",
            LocationId = "store-1",
            Date = new DateOnly(2026, 7, 3),
            SlotStart = new TimeOnly(9, 0),
            SlotEnd = new TimeOnly(10, 0),
            AppointmentReason = Constants.Constants.StoreManagerBlockedAppointmentReason
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetStoreSlotsAsync(dbContext, "store-1", new DateOnly(2026, 7, 3));

        Assert.False(Assert.Single(results).IsAvailable);
    }

    [Fact]
    public async Task StoreSlotsByDate_ShouldReturnAvailable_WhenOverlappingAppointmentIsCancelled()
    {
        await using var dbContext = CreateDbContext(new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero));
        dbContext.Stores.Add(BuildStore("store-1", "Fri"));
        dbContext.StoreSlots.Add(BuildSlot("store-1", "Fri", new TimeOnly(9, 0), new TimeOnly(10, 0)));
        dbContext.StoreAppointments.Add(new Appointment
        {
            Id = "appointment-1",
            LocationId = "store-1",
            Date = new DateOnly(2026, 7, 3),
            SlotStart = new TimeOnly(9, 0),
            SlotEnd = new TimeOnly(10, 0),
            AppointmentStatus = Constants.Constants.CancelledAppointmentStatus
        });
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var results = await GetStoreSlotsAsync(dbContext, "store-1", new DateOnly(2026, 7, 3));

        Assert.True(Assert.Single(results).IsAvailable);
    }

    private static StoreAppointmentDbContext CreateDbContext(DateTimeOffset utcNow)
    {
        var options = new DbContextOptionsBuilder<StoreAppointmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StoreAppointmentDbContext(options, new FixedTimeProvider(utcNow));
    }

    private static Task<List<StoreCalendarAvailabilityResultRow>> GetCalendarAvailabilityAsync(
        StoreAppointmentDbContext dbContext,
        string storeId,
        int year,
        int month)
    {
        return dbContext.ExecuteStoredProcedureAsync<StoreCalendarAvailabilityResultRow>(
            StoredProcedures.StoreCalendarAvailabilityGetByMonth,
            new SqlParameter("@StoreId", storeId),
            new SqlParameter("@Year", year),
            new SqlParameter("@Month", month));
    }

    private static Task<List<StoreSlotResult>> GetStoreSlotsAsync(
        StoreAppointmentDbContext dbContext,
        string storeId,
        DateOnly selectedDate)
    {
        return dbContext.ExecuteStoredProcedureAsync<StoreSlotResult>(
            StoredProcedures.StoreSlotsGetByDate,
            new SqlParameter("@StoreId", storeId),
            new SqlParameter("@SelectedDate", selectedDate));
    }

    private static StoreEntity BuildStore(
        string storeId,
        string dayCode,
        string openingTime = "09:00",
        string closingTime = "17:00") => new()
        {
            StoreId = storeId,
            Name = storeId,
            Label = storeId,
            StoreWorkHoursJson = $$"""[{"day":"{{dayCode}}","openingTime":"{{openingTime}}","closingTime":"{{closingTime}}","isClosed":false}]""",
            StoreSpecialHoursJson = "{\"specialHours\":[]}"
        };

    private static StoreSlotEntity BuildSlot(string storeId, string dayCode, TimeOnly startTime, TimeOnly endTime, int capacity = 1)
    {
        return new StoreSlotEntity
        {
            StoreId = storeId,
            DayCode = dayCode,
            SlotStartTime = startTime,
            SlotEndTime = endTime,
            Capacity = capacity,
            IsActive = true
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
