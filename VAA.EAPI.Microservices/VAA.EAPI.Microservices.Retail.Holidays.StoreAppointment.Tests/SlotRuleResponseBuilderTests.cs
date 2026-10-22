using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public sealed class SlotRuleResponseHelperTests
{
    [Fact]
    public void Build_ShouldAddDefaultRule_WhenDayChangesFromClosedToOpenWithoutExistingRule()
    {
        var openingHours = new[]
        {
            OpeningHours(dayOfWeek: 1, opensAt: "09:00", closesAt: "18:00"),
            OpeningHours(dayOfWeek: 2, opensAt: "09:00", closesAt: "19:00")
        };
        var slotRules = new[]
        {
            SlotRule(dayOfWeek: 2, startTime: "10:00", endTime: "17:00", duration: 30, capacity: 2)
        };

        var result = SlotRuleResponseHelper.Build("1043", openingHours, slotRules).ToArray();

        Assert.Collection(result,
            monday =>
            {
                Assert.Equal("1043-1-0001", monday.Id);
                Assert.Equal(1, monday.DayOfWeek);
                Assert.Equal("09:00", monday.StartTime);
                Assert.Equal("18:00", monday.EndTime);
                Assert.Equal(60, monday.SlotDurationMinutes);
                Assert.Equal(1, monday.SlotCapacity);
            },
            tuesday =>
            {
                Assert.Equal(2, tuesday.DayOfWeek);
                Assert.Equal("10:00", tuesday.StartTime);
                Assert.Equal("17:00", tuesday.EndTime);
                Assert.Equal(30, tuesday.SlotDurationMinutes);
                Assert.Equal(2, tuesday.SlotCapacity);
            });
    }

    [Fact]
    public void Build_ShouldOmitRulesForClosedDays()
    {
        var openingHours = new[]
        {
            OpeningHours(dayOfWeek: 1, opensAt: "00:00", closesAt: "00:00", isClosed: true),
            OpeningHours(dayOfWeek: 2, opensAt: "09:00", closesAt: "19:00")
        };
        var slotRules = new[]
        {
            SlotRule(dayOfWeek: 1, startTime: "09:00", endTime: "18:00"),
            SlotRule(dayOfWeek: 2, startTime: "09:00", endTime: "19:00")
        };

        var result = SlotRuleResponseHelper.Build("1043", openingHours, slotRules);

        var rule = Assert.Single(result);
        Assert.Equal(2, rule.DayOfWeek);
    }

    [Fact]
    public void Build_ShouldPreserveMultipleExistingRulesForAnOpenDay()
    {
        var openingHours = new[]
        {
            OpeningHours(dayOfWeek: 3, opensAt: "09:00", closesAt: "20:00")
        };
        var slotRules = new[]
        {
            SlotRule(dayOfWeek: 3, startTime: "14:00", endTime: "20:00"),
            SlotRule(dayOfWeek: 3, startTime: "09:00", endTime: "13:00")
        };

        var result = SlotRuleResponseHelper.Build("1043", openingHours, slotRules).ToArray();

        Assert.Equal(2, result.Length);
        Assert.Equal("09:00", result[0].StartTime);
        Assert.Equal("14:00", result[1].StartTime);
    }

    [Fact]
    public void Build_ShouldReturnExistingRules_WhenOpeningHoursAreUnavailable()
    {
        var slotRules = new[]
        {
            SlotRule(dayOfWeek: 4, startTime: "09:00", endTime: "18:00")
        };

        var result = SlotRuleResponseHelper.Build("1043", Array.Empty<OpeningHoursEntryResponse>(), slotRules);

        var rule = Assert.Single(result);
        Assert.Equal(4, rule.DayOfWeek);
    }

    private static OpeningHoursEntryResponse OpeningHours(int dayOfWeek, string opensAt, string closesAt, bool isClosed = false) => new()
    {
        DayOfWeek = dayOfWeek,
        OpensAt = isClosed ? "closed" : opensAt,
        ClosesAt = isClosed ? "closed" : closesAt
    };

    private static SlotRuleResponse SlotRule(int dayOfWeek, string startTime, string endTime, int duration = 60, int capacity = 1) => new()
    {
        Id = $"1043-{dayOfWeek}-0001",
        DayOfWeek = dayOfWeek,
        StartTime = startTime,
        EndTime = endTime,
        SlotDurationMinutes = duration,
        SlotCapacity = capacity
    };
}