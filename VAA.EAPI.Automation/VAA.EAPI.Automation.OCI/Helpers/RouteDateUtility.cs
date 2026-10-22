namespace VAA.EAPI.Automation.OCI.Helpers
{
    public static class RouteDateUtility
    {
        private record RouteServicePeriod(DateTime StartDate, DateTime? EndDate, DayOfWeek[] OperatingDays);

        private record RouteRule(string Origin, string Destination, RouteServicePeriod[] ServicePeriods, DayOfWeek[]? ReturnDays = null);

        public record TravelDates(DateTime DepartureDate, DateTime ReturnDate);

        private static readonly RouteRule LhrHkt = new(
            "LHR",
            "HKT",
            [
                new(
                new DateTime(2026, 10, 18),
                null,
                [
                    DayOfWeek.Wednesday,
                    DayOfWeek.Friday,
                    DayOfWeek.Sunday
                ])
            ],
            [
                DayOfWeek.Monday,
                DayOfWeek.Thursday,
                DayOfWeek.Saturday
            ]);

        private static readonly RouteRule LhrCun = new(
            "LHR",
            "CUN",
            [
                new(
                new DateTime(2026, 10, 25),
                new DateTime(2027, 4, 11),
                [
                    DayOfWeek.Wednesday,
                    DayOfWeek.Friday,
                    DayOfWeek.Sunday
                ])
            ]);

        private static readonly RouteRule LhrMle = new(
            "LHR",
            "MLE",
            [
                new(
                new DateTime(2026, 10, 27),
                new DateTime(2026, 11, 30),
                [
                    DayOfWeek.Tuesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Saturday
                ]),

            new(
                new DateTime(2026, 12, 1),
                null,
                Enum.GetValues<DayOfWeek>())
            ],
            [
                DayOfWeek.Wednesday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            ]);

        private static readonly RouteRule LhrCpt = new(
            "LHR",
            "CPT",
            [
                new(
                new DateTime(2026, 10, 15),
                new DateTime(2027, 4, 20),
                Enum.GetValues<DayOfWeek>())
            ]);

        private static readonly RouteRule ManLas = new(
            "MAN",
            "LAS",
            [
                new(
                new DateTime(2026, 4, 21),
                new DateTime(2026, 10, 23),
                [
                    DayOfWeek.Tuesday,
                    DayOfWeek.Friday,
                    DayOfWeek.Sunday
                ])
            ]);

        private static readonly RouteRule ManBgi = new(
            "MAN",
            "BGI",
            [
                new(
                new DateTime(2026, 10, 26),
                new DateTime(2027, 3, 19),
                [
                    DayOfWeek.Monday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Sunday
                ])
            ]);

        private static readonly RouteRule ManMbj = new(
            "MAN",
            "MBJ",
            [
                new(
                new DateTime(2026, 7, 15),
                new DateTime(2026, 9, 4),
                Enum.GetValues<DayOfWeek>()),

            new(
                new DateTime(2026, 9, 5),
                null,
                [
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Saturday
                ])
            ]);

        public static TravelDates GetTravelDates(string origin, string destination, DateTime fromDate)
        {
            RouteRule route = GetRoute(origin, destination);

            DateTime departureDate = GetNextOutboundDate(route, fromDate);

            DateTime? returnDate = null;

            if (route.ReturnDays != null)
            {
                // Overnight flight: return-date calculation starts
                // from the following calendar day.
                DateTime earliestReturnDate = departureDate.AddDays(1);

                returnDate = GetNextReturnDate(route.ReturnDays, earliestReturnDate);
            }

            return new TravelDates(departureDate, returnDate ?? departureDate);
        }

        private static DateTime GetNextOutboundDate(RouteRule route, DateTime fromDate)
        {
            DateTime date = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Unspecified);

            foreach (RouteServicePeriod period in route.ServicePeriods)
            {
                if (date < period.StartDate)
                {
                    date = period.StartDate;
                }

                while (period.EndDate == null || date <= period.EndDate)
                {
                    if (period.OperatingDays.Contains(date.DayOfWeek))
                    {
                        return date;
                    }

                    date = date.AddDays(1);
                }
            }

            throw new InvalidOperationException(
                $"No valid departure date found for {route.Origin}-{route.Destination}.");
        }

        private static DateTime GetNextReturnDate(DayOfWeek[] returnDays, DateTime fromDate)
        {
            DateTime date = fromDate;

            while (!returnDays.Contains(date.DayOfWeek))
            {
                date = date.AddDays(1);
            }

            return date;
        }

        private static RouteRule GetRoute(string origin, string destination)
        {
            return (origin, destination) switch
            {
                ("LHR", "HKT") => LhrHkt,
                ("LHR", "CUN") => LhrCun,
                ("LHR", "MLE") => LhrMle,
                ("LHR", "CPT") => LhrCpt,
                ("MAN", "LAS") => ManLas,
                ("MAN", "BGI") => ManBgi,
                ("MAN", "MBJ") => ManMbj,

                _ => throw new ArgumentException(
                    $"Route {origin}-{destination} is not configured.")
            };
        }
    }
}
