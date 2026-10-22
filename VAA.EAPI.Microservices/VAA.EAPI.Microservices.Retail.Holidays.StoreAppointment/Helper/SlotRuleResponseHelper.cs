using System.Globalization;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;

public static class SlotRuleResponseHelper
{
    private const int DefaultSlotDurationMinutes = 60;
    private const int DefaultSlotCapacity = 1;

    public static IReadOnlyCollection<SlotRuleResponse> Build(string storeId, IReadOnlyCollection<OpeningHoursEntryResponse> openingHours, IReadOnlyCollection<SlotRuleResponse> slotRules)
    {
        if (openingHours.Count == 0)
        {
            return OrderRules(slotRules);
        }

        var openDays = openingHours
            .Where(IsOpen)
            .GroupBy(hours => ValidateDayOfWeek(hours.DayOfWeek))
            .ToDictionary(group => group.Key, group => group.First());

        var rulesByDay = slotRules
            .Where(rule => openDays.ContainsKey(ValidateDayOfWeek(rule.DayOfWeek)))
            .GroupBy(rule => rule.DayOfWeek)
            .ToDictionary(group => group.Key, group => group.ToArray());

        var result = new List<SlotRuleResponse>();
        foreach (var (dayOfWeek, hours) in openDays.OrderBy(item => item.Key))
        {
            if (rulesByDay.TryGetValue(dayOfWeek, out var existingRules))
            {
                result.AddRange(existingRules);
                continue;
            }

            result.Add(new SlotRuleResponse
            {
                Id = $"{storeId}-{dayOfWeek}-0001",
                DayOfWeek = dayOfWeek,
                StartTime = hours.OpensAt,
                EndTime = hours.ClosesAt,
                SlotDurationMinutes = DefaultSlotDurationMinutes,
                SlotCapacity = DefaultSlotCapacity
            });
        }

        return OrderRules(result);
    }

    private static bool IsOpen(OpeningHoursEntryResponse hours)
    {
        if (string.Equals(hours.OpensAt, "closed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hours.ClosesAt, "closed", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ParseTime(hours.OpensAt) < ParseTime(hours.ClosesAt);
    }

    private static IReadOnlyCollection<SlotRuleResponse> OrderRules(IEnumerable<SlotRuleResponse> rules) =>
        rules.OrderBy(rule => ValidateDayOfWeek(rule.DayOfWeek))
            .ThenBy(rule => rule.StartTime)
            .ToArray();

    private static int ValidateDayOfWeek(int dayOfWeek)
    {
        if (dayOfWeek is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "DayOfWeek must be between 0 and 6.");
        }

        return dayOfWeek;
    }

    private static TimeSpan ParseTime(string time) =>
        TimeSpan.ParseExact(time, @"hh\:mm", CultureInfo.InvariantCulture);
}
