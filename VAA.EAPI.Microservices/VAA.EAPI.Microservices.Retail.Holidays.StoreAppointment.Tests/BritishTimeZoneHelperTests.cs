using System;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public class BritishTimeZoneHelperTests
{
    [Fact]
    public void BuildStoreLocalDateTimeOffset_ShouldHandleUtcDateKindWithoutThrowing()
    {
        var selectedDateUtc = new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc);

        var result = BritishTimeZoneHelper.BuildStoreLocalDateTimeOffset(selectedDateUtc, new TimeOnly(9, 0));

        Assert.Equal(new DateTime(2026, 4, 20, 9, 0, 0), result.DateTime);
        Assert.Equal(TimeSpan.FromHours(1), result.Offset);
    }

    [Fact]
    public void BuildStoreLocalDateTimeOffset_ShouldReturnGmtOffsetInWinter()
    {
        var selectedDateUtc = new DateTime(2026, 12, 16, 0, 0, 0, DateTimeKind.Utc);

        var result = BritishTimeZoneHelper.BuildStoreLocalDateTimeOffset(selectedDateUtc, new TimeOnly(9, 0));

        Assert.Equal(TimeSpan.Zero, result.Offset);
    }
    [Fact]
    public void InterpretDatabaseClockAsUkLocal_ShouldPreserveSummerSlotClockTime()
    {
        var databaseClock = new DateTimeOffset(2026, 6, 18, 15, 0, 0, TimeSpan.Zero);

        var result = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(databaseClock);

        Assert.Equal(new DateTime(2026, 6, 18, 15, 0, 0), result.DateTime);
        Assert.Equal(TimeSpan.FromHours(1), result.Offset);
    }

    [Fact]
    public void InterpretDatabaseClockAsUkLocal_ShouldPreserveWinterSlotClockTime()
    {
        var databaseClock = new DateTimeOffset(2026, 12, 18, 15, 0, 0, TimeSpan.Zero);

        var result = BritishTimeZoneHelper.InterpretDatabaseClockAsUkLocal(databaseClock);

        Assert.Equal(new DateTime(2026, 12, 18, 15, 0, 0), result.DateTime);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }
}
