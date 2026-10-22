using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;

namespace VAA.EAPI.GraphQL.Domain.Search.Services
{

    internal static class RewardSeatAvailabilityWorkUnitPartitioner
    {
        internal static IEnumerable<RewardSeatAvailabilityWorkUnit> Create(
            int year,
            RewardSeatMonth month,
            DateOnly today,
            int maxAdvanceDays,
            int maximumWorkUnits)
        {
            if (maxAdvanceDays is < 1 or > 366)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAdvanceDays));
            }

            if (maximumWorkUnits < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumWorkUnits));
            }

            var monthStart = new DateOnly(year, (int)month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var horizonEnd = today.AddDays(maxAdvanceDays);
            var intervalStart = monthStart > today ? monthStart : today;
            var intervalEnd = monthEnd < horizonEnd ? monthEnd : horizonEnd;

            if (intervalStart > intervalEnd)
            {
                throw new ArgumentException("The requested month is outside the booking horizon.", nameof(month));
            }

            var anchorDays = new HashSet<DateOnly>
            {
                intervalStart,
                intervalEnd
            };

            foreach (var day in new[] { 1, 10, 20, 28 })
            {
                var anchor = new DateOnly(year, (int)month, day);
                if (anchor >= intervalStart && anchor <= intervalEnd)
                {
                    anchorDays.Add(anchor);
                }
            }

            if (today >= intervalStart && today <= intervalEnd)
            {
                anchorDays.Add(today);
            }

            var workUnits = anchorDays
                .OrderBy(anchor => anchor)
                .Select(anchor => new RewardSeatAvailabilityWorkUnit
                {
                    AnchorDate = anchor,
                    IntervalStart = intervalStart,
                    IntervalEnd = intervalEnd
                })
                .ToArray();

            if (workUnits.Length > maximumWorkUnits)
            {
                throw new InvalidOperationException("The requested calendar window exceeds the configured work-unit limit.");
            }

            return workUnits;
        }
    }
}
