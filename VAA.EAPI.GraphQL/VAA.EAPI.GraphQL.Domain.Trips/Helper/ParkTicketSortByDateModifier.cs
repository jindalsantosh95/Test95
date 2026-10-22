using NodaTime;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    /// <summary>
    /// Park Tickets (specific types of <see cref="Extra"/>) can have a start date set to the start of the holiday.
    /// In order to position them in the correct place in the list of holiday components, the Orlando hotels are found,
    /// and the <see cref="Product.SortByDateInternal"/> value on each of the tickets is set to after the hotel's check-in time.
    ///
    /// If no Orlando hotel can be found, we fall back to ordering after the first flight slice that arrives into MCO.
    ///
    /// This is only done if the ticket start date is before the first Orlando hotel start date or if the ticket starts
    /// on the same day as the check-in. If it is not, then the start date of the ticket is used for ordering.
    ///
    /// Ordering tickets after the first Orlando hotel can be done as tickets will always be used whilst staying at a hotel in Orlando.
    /// </summary>
    internal static class ParkTicketSortByDateModifier
    {
        private static readonly string[] ParkTicketTokens = ["DISNEY", "UNIV", "UNTWO", "UNIVFC", "UNIVFC3"];

        internal static void Apply(IList<Product> products)
        {
            if (products is null || products.Count == 0)
                return;

            var firstOrlandoHotelCheckIn = GetFirstOrlandoHotelCheckIn(products);
            if (firstOrlandoHotelCheckIn is null)
            {
                // No Orlando hotel in the itinerary - fall back to the first flight arriving into MCO (Orlando).
                firstOrlandoHotelCheckIn = GetFirstMcoFlightArrival(products);
            }

            if (firstOrlandoHotelCheckIn is null)
                return;

            var checkIn = firstOrlandoHotelCheckIn.Value;
            if (checkIn == default)
                return;

            // Make the change deterministic: order tickets first, then add a small increasing offset.
            var candidateTickets = products
                .Where(IsParkTicket)
                .Select(p => new { Product = p, Start = GetTicketStartDate(p) })
                .Where(x => x.Start is not null)
                .OrderBy(x => x.Start)
                .ThenBy(x => x.Product.TbxProductId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Product.ExtraDetails?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (candidateTickets.Count == 0)
                return;

            var baseSortTime = checkIn.Plus(Period.FromMinutes(1));
            var adjusted = 0;

            foreach (var ticket in candidateTickets)
            {
                var start = ticket.Start!.Value;

                // Only adjust when ticket start is before the first Orlando hotel start date, or on the same day.
                if (start.Date > checkIn.Date)
                    continue;

                var newSortBy = baseSortTime.Plus(Period.FromSeconds(adjusted));
                ticket.Product.SortByDateInternal = newSortBy;              

                adjusted++;
            }
        }

        internal static LocalDateTime? GetFirstOrlandoHotelCheckIn(IEnumerable<Product> products)
        {
            if (products is null)
                return null;

            return products
                .Where(p => p?.HotelDetails is not null)
                .Select(p => p!.HotelDetails!)
                .Where(IsOrlandoHotel)
                .Select(h => (LocalDateTime?)h.CheckIn)
                .Where(dt => dt is not null && dt.Value != default)
                .OrderBy(dt => dt)
                .FirstOrDefault();
        }

        internal static LocalDateTime? GetFirstMcoFlightArrival(IEnumerable<Product> products)
        {
            if (products is null)
                return null;

            // Products are normalized to one slice per flight product in HolidayQuery.GetOrderedProducts.
            return products
                .Where(p => p?.Slices is not null && p.Slices.Any())
                .SelectMany(p => p!.Slices!)
                .Where(s => s?.Destination?.Code is not null && s.Destination.Code.Equals("MCO", StringComparison.OrdinalIgnoreCase))
                .Select(s => (LocalDateTime?)(s.ScheduledArrivalLocal))
                .Where(dt => dt is not null && dt.Value != default)
                .OrderBy(dt => dt)
                .FirstOrDefault();
        }

        internal static bool IsOrlandoHotel(HotelInformation hotel)
        {
            if (hotel is null)
                return false;

            // We don't have a single canonical "Orlando" marker, so we check the most reliable populated fields.
            return ContainsOrlando(hotel.Destination?.CityName)
                || ContainsOrlando(hotel.HotelAddress?.City)
                || ContainsOrlando(hotel.Destination?.CityCode);                
        }

        internal static bool IsParkTicket(Product product)
        {
            if (product?.ExtraDetails is null)
                return false;

            if (product.ExtraDetails.IsDisneyTicket == true)
                return true;

            // Fallback: match on a few text fields we already have available.
            return ContainsAnyToken(product.ExtraDetails.ElementGroup)
                || ContainsAnyToken(product.ExtraDetails.ElementGroupName)
                || ContainsAnyToken(product.ExtraDetails.Name);
        }

        internal static LocalDateTime? GetTicketStartDate(Product product)
        {
            if (product is null)
                return null;

            if (product.ExtraDetails?.StartDate is not null && product.ExtraDetails.StartDate.Value != default)
                return product.ExtraDetails.StartDate;

            var sort = product.SortByDateInternal ?? product.SortByDate;
            return sort == default ? null : sort;
        }

        private static bool ContainsOrlando(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            (value.Contains("ORLANDO", StringComparison.OrdinalIgnoreCase) || value.Contains("ORL", StringComparison.OrdinalIgnoreCase));

        private static bool ContainsAnyToken(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            ParkTicketTokens.Any(t => value.Contains(t, StringComparison.OrdinalIgnoreCase));
    }
}
