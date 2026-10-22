using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    /// <summary>
    /// There are scenarios where hotel check-in times cannot be used for itinerary ordering:
    /// 1) When the holiday includes an internal flight, some flights depart after the hotel check-in time.
    ///    Sorting by flight departure and hotel check-in would place the hotel before the flight (hotel -> flight).
    /// 2) Hotel check-in times can be set to midnight (00:00). When a holiday includes flights, this can also
    ///    cause the hotel to be ordered before flights departing later that day.
    ///
    /// This modifier corrects ordering by:
    /// - For hotels whose check-in date matches any flight departure date, setting the hotel's SortByDateInternal
    ///   to after the latest flight departure on that date (keeping flight -> hotel).
    /// - Adjusting midnight check-in times to 3pm (the default TBX check-in time). For hotels not adjusted by the flight rule,
    ///   this also updates SortByDateInternal when it was at midnight.
    /// </summary>
    internal static class HotelSortByDateModifier
    {
        private static readonly LocalTime DefaultCheckInTime = new(15, 0);

        internal static void Apply(IList<Product> products)
        {
            if (products is null || products.Count == 0)
                return;

            var hotels = products.Where(p => p?.HotelDetails is not null).ToList();
            if (hotels.Count == 0)
                return;

            var flights = products
                .Where(IsFlight)
                .Select(f => new { Flight = f, Departure = GetFlightDepartureTime(f) })
                .Where(x => x.Departure is not null && x.Departure.Value != default)
                .ToList();

            var adjustedForFlight = new HashSet<Product>();

            if (flights.Count > 0)
            {
                var latestFlightDepartureByDate = flights
                    .GroupBy(x => x.Departure!.Value.Date)
                    .ToDictionary(g => g.Key, g => g.Max(x => x.Departure!.Value));

                foreach (var hotelProduct in hotels)
                {
                    var checkIn = GetHotelCheckIn(hotelProduct);
                    if (checkIn is null || checkIn.Value == default)
                        continue;

                    if (!latestFlightDepartureByDate.TryGetValue(checkIn.Value.Date, out var latestDeparture))
                        continue;

                    var currentSort = hotelProduct.SortByDateInternal ?? hotelProduct.SortByDate;
                    if (currentSort == default)
                        continue;

                    // Only adjust when the hotel currently sorts before (or equal to) the latest departure on the check-in date.
                    if (currentSort.CompareTo(latestDeparture) > 0)
                        continue;

                    var newSort = latestDeparture.Plus(Period.FromMinutes(10));
                    hotelProduct.SortByDateInternal = newSort;
                   
                    adjustedForFlight.Add(hotelProduct);
                }
            }

            foreach (var hotelProduct in hotels)
            {
                var hotel = hotelProduct.HotelDetails!;
                if (hotel.CheckIn == default || hotel.CheckIn.TimeOfDay != LocalTime.Midnight)
                    continue;

                var adjustedCheckIn = hotel.CheckIn.Date.At(DefaultCheckInTime);
                hotel.CheckIn = adjustedCheckIn;

                // Only update the hotel sort-by when it wasn't already adjusted to follow flights.
                if (adjustedForFlight.Contains(hotelProduct))
                    continue;

                var currentSort = hotelProduct.SortByDateInternal ?? hotelProduct.SortByDate;
                if (currentSort == default)
                    continue;

                if (currentSort.Date == adjustedCheckIn.Date && currentSort.TimeOfDay == LocalTime.Midnight)
                {
                    hotelProduct.SortByDateInternal = adjustedCheckIn;                    
                }
            }
        }

        private static bool IsFlight(Product product) =>
            string.Equals(product?.BookingType, "AIRLINE", StringComparison.OrdinalIgnoreCase)
            && product.Slices?.Any() == true;

        private static LocalDateTime? GetFlightDepartureTime(Product flight)
        {
            var slice = flight?.Slices?.FirstOrDefault();
            return slice?.ScheduledDepartureLocal
                   ?? slice?.EstimatedDepartureLocal
                   ?? slice?.ActualDepartureLocal
                   ?? flight?.SortByDateInternal;
        }

        private static LocalDateTime? GetHotelCheckIn(Product hotelProduct)
        {
            if (hotelProduct?.HotelDetails is not null && hotelProduct.HotelDetails.CheckIn != default)
                return hotelProduct.HotelDetails.CheckIn;

            var sort = hotelProduct?.SortByDateInternal ?? hotelProduct?.SortByDate;
            return sort == default ? null : sort;
        }
    }
}

