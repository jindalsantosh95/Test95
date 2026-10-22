using System;
using System.Collections.Generic;
using System.Reflection;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public sealed class StoreManagerBookingConflictTests
{
    private const string ConflictTypeName =
        "VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.StoreManagerBookingConflict";
    private const string ConflictRowTypeName =
        "VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models.StoreManagerBookingConflictRow";

    [Fact]
    public void Create_ShouldReturnNull_WhenThereAreNoConflicts()
    {
        var result = CreateConflict("store-1");

        Assert.Null(result);
    }

    [Fact]
    public void Create_ShouldMapSingleConflict()
    {
        var result = CreateConflict(
            "store-1",
            new ConflictRow(
                new DateOnly(2026, 7, 27),
                new DateOnly(2027, 7, 27),
                new DateOnly(2026, 7, 29),
                2,
                "OUTSIDE_PROPOSED_OPENING_HOURS"));

        Assert.NotNull(result);
        Assert.True(result.IsConflict);
        Assert.Equal("ACTIVE_BOOKINGS_EXIST", result.ErrorCode);
        Assert.Equal(
            "The requested change cannot be saved because it would affect active bookings.",
            result.ErrorMessage);

        var details = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result.ErrorDetails);
        Assert.Equal("store-1", details["storeId"]);
        Assert.Equal("2026-07-27", details["rangeStart"]);
        Assert.Equal("2027-07-27", details["rangeEndExclusive"]);
        Assert.Equal(2, details["activeBookingCount"]);
        Assert.Equal("OUTSIDE_PROPOSED_OPENING_HOURS", details["reason"]);

        var affectedDate = Assert.Single(Assert.IsType<Dictionary<string, object?>[]>(details["affectedDates"]));
        Assert.Equal("2026-07-29", affectedDate["date"]);
        Assert.Equal(2, affectedDate["activeBookingCount"]);
    }

    [Fact]
    public void Create_ShouldGroupSumAndOrderAffectedDates()
    {
        var result = CreateConflict(
            "store-1",
            new ConflictRow(
                new DateOnly(2026, 8, 1),
                new DateOnly(2027, 8, 1),
                new DateOnly(2026, 7, 30),
                1,
                "LATER_CONFLICT"),
            new ConflictRow(
                new DateOnly(2026, 7, 27),
                new DateOnly(2027, 7, 27),
                new DateOnly(2026, 7, 29),
                2,
                "NO_MATCHING_SLOT_RULE"),
            new ConflictRow(
                new DateOnly(2026, 7, 27),
                new DateOnly(2027, 7, 27),
                new DateOnly(2026, 7, 29),
                3,
                "NO_MATCHING_SLOT_RULE"));

        var details = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(result!.ErrorDetails);
        Assert.Equal(6, details["activeBookingCount"]);
        Assert.Equal("2026-07-27", details["rangeStart"]);
        Assert.Equal("2027-07-27", details["rangeEndExclusive"]);
        Assert.Equal("NO_MATCHING_SLOT_RULE", details["reason"]);

        var affectedDates = Assert.IsType<Dictionary<string, object?>[]>(details["affectedDates"]);
        Assert.Collection(
            affectedDates,
            affectedDate =>
            {
                Assert.Equal("2026-07-29", affectedDate["date"]);
                Assert.Equal(5, affectedDate["activeBookingCount"]);
            },
            affectedDate =>
            {
                Assert.Equal("2026-07-30", affectedDate["date"]);
                Assert.Equal(1, affectedDate["activeBookingCount"]);
            });
    }

    private static ServiceResult<object>? CreateConflict(string storeId, params ConflictRow[] conflicts)
    {
        var assembly = typeof(StoreManagerRepository).Assembly;
        var conflictType = assembly.GetType(ConflictTypeName, throwOnError: true)!;
        var conflictRowType = assembly.GetType(ConflictRowTypeName, throwOnError: true)!;
        var conflictRows = Array.CreateInstance(conflictRowType, conflicts.Length);

        for (var index = 0; index < conflicts.Length; index++)
        {
            var conflictRow = Activator.CreateInstance(conflictRowType)!;
            SetProperty(conflictRowType, conflictRow, "RangeStart", conflicts[index].RangeStart);
            SetProperty(conflictRowType, conflictRow, "RangeEndExclusive", conflicts[index].RangeEndExclusive);
            SetProperty(conflictRowType, conflictRow, "Date", conflicts[index].Date);
            SetProperty(conflictRowType, conflictRow, "ActiveBookingCount", conflicts[index].ActiveBookingCount);
            SetProperty(conflictRowType, conflictRow, "Reason", conflicts[index].Reason);
            conflictRows.SetValue(conflictRow, index);
        }

        var createMethod = conflictType
            .GetMethod("Create", BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(typeof(object));

        return (ServiceResult<object>?)createMethod.Invoke(null, [storeId, conflictRows]);
    }

    private static void SetProperty(Type type, object instance, string propertyName, object value) =>
        type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!.SetValue(instance, value);

    private sealed record ConflictRow(
        DateOnly RangeStart,
        DateOnly RangeEndExclusive,
        DateOnly Date,
        int ActiveBookingCount,
        string Reason);
}
