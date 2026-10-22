namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    public static class SeasonalRouteDateResolver
    {
        private sealed record ServicePeriod(DateTime Start, DateTime? End, DayOfWeek[] Days);

        private static readonly IReadOnlyDictionary<string, ServicePeriod[]> Rules =
            new Dictionary<string, ServicePeriod[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["LHR-HKT"] =
                [
                    new(new DateTime(2026, 10, 18), null,
                        [DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Sunday]),
                ],
                ["LHR-CUN"] =
                [
                    new(new DateTime(2026, 10, 25), new DateTime(2027, 4, 11),
                        [DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Sunday]),
                ],
                ["LHR-MLE"] =
                [
                    new(new DateTime(2026, 10, 27), new DateTime(2026, 11, 30),
                        [DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Saturday]),
                    new(new DateTime(2026, 12, 1), null, Enum.GetValues<DayOfWeek>()),
                ],
                ["LHR-CPT"] =
                [
                    new(new DateTime(2026, 10, 15), new DateTime(2027, 4, 20), Enum.GetValues<DayOfWeek>()),
                ],
                ["MAN-LAS"] =
                [
                    new(new DateTime(2026, 4, 21), new DateTime(2026, 10, 23),
                        [DayOfWeek.Tuesday, DayOfWeek.Friday, DayOfWeek.Sunday]),
                ],
                ["MAN-BGI"] =
                [
                    new(new DateTime(2026, 10, 26), new DateTime(2027, 3, 19),
                        [DayOfWeek.Monday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Sunday]),
                ],
                ["MAN-MBJ"] =
                [
                    new(new DateTime(2026, 7, 15), new DateTime(2026, 9, 4), Enum.GetValues<DayOfWeek>()),
                    new(new DateTime(2026, 9, 5), null,
                        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Saturday]),
                ],
            };

        public static DateTime Resolve(string origin, string destination, DateTime fromDate)
        {
            string key = $"{origin}-{destination}";
            if (!Rules.TryGetValue(key, out ServicePeriod[]? periods))
                throw new InvalidOperationException($"Seasonal route {key} is not configured.");

            DateTime date = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Unspecified);
            foreach (ServicePeriod period in periods)
            {
                if (date < period.Start)
                    date = period.Start;

                while (!period.End.HasValue || date <= period.End.Value)
                {
                    if (period.Days.Contains(date.DayOfWeek))
                        return date;
                    date = date.AddDays(1);
                }
            }

            throw new InvalidOperationException($"No valid departure date found for seasonal route {key}.");
        }

        public static bool IsConfigured(string origin, string destination) =>
            Rules.ContainsKey($"{origin}-{destination}");

        public static IReadOnlyList<string> GetRoutesWithoutFutureService(DateTime fromDate) =>
            Rules
                .Where(rule => rule.Value.All(period => period.End.HasValue && period.End.Value < fromDate.Date))
                .Select(rule => rule.Key)
                .OrderBy(route => route, StringComparer.OrdinalIgnoreCase)
                .ToArray();

        public static bool TryResolve(string origin, string destination, DateTime fromDate, out DateTime departureDate)
        {
            try
            {
                departureDate = Resolve(origin, destination, fromDate);
                return true;
            }
            catch (InvalidOperationException)
            {
                departureDate = default;
                return false;
            }
        }
    }
}
