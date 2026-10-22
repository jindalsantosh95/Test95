using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    /// <summary>
    /// Transfers are commonly returned with a start time set to 00:00:00 which breaks date/time ordering.
    ///
    /// A transfer is usually either:
    /// - pickup-from-airport (airport -> hotel)
    /// - drop-off-at-airport (hotel -> airport)
    ///
    /// To support customer-facing ordering, transfer sort times are aligned relative to the matching flight.
    /// For matching, the transfer airport code is taken from whichever end of the transfer is identified as an
    /// airport (pickup for airport -> hotel, drop-off for hotel -> airport), and the flight is selected using that
    /// airport code together with the relevant flight date logic in this class.
    ///
    /// Pickup-from-airport transfers (airport -> hotel) are set to 5 minutes after the matching flight's departure
    /// time so the ordering remains flight -> transfer -> hotel.
    ///
    /// Drop-off-at-airport transfers (hotel -> airport) are aligned to just before the departing flight so the ordering is: hotel -> transfer -> flight.
    /// </summary>
    internal static class TransferSortByDateModifier
    {
        internal static void Apply(IList<Product> products)
        {
            if (products is null || products.Count == 0)
                return;

            var flights = products.Where(IsFlight).ToList();
            if (flights.Count == 0)
                return;

            foreach (var transfer in products.Where(IsTransfer))
            {
                var currentSort = transfer.SortByDateInternal ?? transfer.SortByDate;
                if (currentSort == default || currentSort.TimeOfDay != LocalTime.Midnight)
                    continue;

                var transferDate = currentSort.Date;

                var pickupIsAirport = IsAirportLocation(transfer.ExtraDetails?.SelectedPickupPoint);
                var dropoffIsAirport = IsAirportLocation(transfer.ExtraDetails?.SelectedDropoffPoint);

                // Never reposition airport <-> airport transfers.
                if (pickupIsAirport && dropoffIsAirport)
                    continue;

                // Only reposition when exactly one end is an airport.
                if (pickupIsAirport == dropoffIsAirport)
                    continue;

                var airportCode = pickupIsAirport
                    ? GetAirportCode(transfer.ExtraDetails?.SelectedPickupPoint)
                    : GetAirportCode(transfer.ExtraDetails?.SelectedDropoffPoint);

                if (string.IsNullOrWhiteSpace(airportCode))
                    continue;

                // Pickup-from-airport (airport -> hotel): align to arriving flight at the airport.
                if (pickupIsAirport)
                {
                    if (!HasFlightArrivingAtAirportOnDate(flights, airportCode, transferDate))
                        continue;

                    var matchingFlight = FindArrivalFlight(flights, airportCode, transferDate);
                    if (matchingFlight is null)
                        continue;

                    var flightDeparture = GetFlightDepartureTime(matchingFlight);
                    if (flightDeparture is null || flightDeparture.Value == default)
                        continue;

                    // Default: order after the flight so the flight remains above the transfer.
                    var newSortBy = flightDeparture.Value.Plus(Period.FromMinutes(5));

                    transfer.SortByDateInternal = newSortBy;                    
                    continue;
                }

                // Drop-off-at-airport (hotel -> airport): align to departing flight at the airport.
                if (!HasFlightDepartingFromAirportOnDate(flights, airportCode, transferDate))
                    continue;

                var departureFlight = FindDepartureFlight(flights, airportCode, transferDate);
                if (departureFlight is null)
                    continue;

                var flightDepartureTime = GetFlightDepartureTime(departureFlight);
                if (flightDepartureTime is null || flightDepartureTime.Value == default)
                    continue;

                // Place transfer just before the flight so it orders above the flight.
                var newDropOffSortBy = flightDepartureTime.Value.Minus(Period.FromMinutes(5));

                transfer.SortByDateInternal = newDropOffSortBy;
            }
        }

        private static bool IsTransfer(Product product) =>
            string.Equals(product?.TbxProductType, "TRS", StringComparison.OrdinalIgnoreCase)
            || string.Equals(product?.BookingType, "TRANSFER", StringComparison.OrdinalIgnoreCase);

        private static bool IsFlight(Product product) =>
            string.Equals(product?.BookingType, "AIRLINE", StringComparison.OrdinalIgnoreCase)
            && product.Slices?.Any() == true;

        private static bool IsAirportLocation(Location? location)
        {
            if (location is null)
                return false;

            return string.Equals(location.Type, "AIRPORT", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(location.AirportCode);
        }

        private static string? GetAirportCode(Location? location)
        {
            if (location is null)
                return null;

            return !string.IsNullOrWhiteSpace(location.AirportCode)
                ? location.AirportCode
                : location.Code;
        }

        private static string? GetArrivalAirportCode(Product flight)
        {
            var slice = flight?.Slices?.FirstOrDefault();
            if (slice is null)
                return null;

            return slice.Segments?.LastOrDefault()?.Destination?.Code
                ?? slice.Destination?.Code;
        }

        private static string? GetDepartureAirportCode(Product flight)
        {
            var slice = flight?.Slices?.FirstOrDefault();
            if (slice is null)
                return null;

            return slice.Segments?.FirstOrDefault()?.Origin?.Code
                ?? slice.Origin?.Code;
        }

        private static bool HasFlightDepartingFromAirportOnDate(IEnumerable<Product> flights, string airportCode, LocalDate date)
        {
            if (flights is null || string.IsNullOrWhiteSpace(airportCode))
                return false;

            return flights.Any(f =>
            {
                if (!string.Equals(GetDepartureAirportCode(f), airportCode, StringComparison.OrdinalIgnoreCase))
                    return false;

                var departureTime = GetFlightDepartureTime(f);
                return departureTime is not null && departureTime.Value != default && departureTime.Value.Date == date;
            });
        }

        private static bool HasFlightArrivingAtAirportOnDate(IEnumerable<Product> flights, string airportCode, LocalDate date)
        {
            if (flights is null || string.IsNullOrWhiteSpace(airportCode))
                return false;

            return flights.Any(f =>
            {
                if (!string.Equals(GetArrivalAirportCode(f), airportCode, StringComparison.OrdinalIgnoreCase))
                    return false;

                var arrivalTime = GetFlightArrivalTime(f);
                if (arrivalTime is not null && arrivalTime.Value != default && arrivalTime.Value.Date == date)
                    return true;

                // Fallback for missing arrival times (timezone gaps/data issues): use departure date.
                var departureTime = GetFlightDepartureTime(f);
                return departureTime is not null && departureTime.Value != default && departureTime.Value.Date == date;
            });
        }

        private static LocalDateTime? GetFlightDepartureTime(Product flight)
        {
            var slice = flight?.Slices?.FirstOrDefault();
            return slice?.ScheduledDepartureLocal ?? flight?.SortByDateInternal;
        }

        private static LocalDateTime? GetFlightArrivalTime(Product flight)
        {
            var slice = flight?.Slices?.FirstOrDefault();
            return slice?.ScheduledArrivalLocal;
        }

        private static Product? FindArrivalFlight(IEnumerable<Product> flights, string airportCode, LocalDate transferDate)
        {
            // Prefer actual arrival-time match.
            var arrival = flights.FirstOrDefault(f =>
            {
                if (!string.Equals(GetArrivalAirportCode(f), airportCode, StringComparison.OrdinalIgnoreCase))
                    return false;

                var arrivalTime = GetFlightArrivalTime(f);
                return arrivalTime is not null && arrivalTime.Value != default && arrivalTime.Value.Date == transferDate;
            });

            if (arrival is not null)
                return arrival;

            // Fallback: match by departure date.
            return flights.FirstOrDefault(f =>
            {
                if (!string.Equals(GetArrivalAirportCode(f), airportCode, StringComparison.OrdinalIgnoreCase))
                    return false;

                var departureTime = GetFlightDepartureTime(f);
                return departureTime is not null && departureTime.Value != default && departureTime.Value.Date == transferDate;
            });
        }

        private static Product? FindDepartureFlight(IEnumerable<Product> flights, string airportCode, LocalDate transferDate)
        {
            return flights.FirstOrDefault(f =>
            {
                if (!string.Equals(GetDepartureAirportCode(f), airportCode, StringComparison.OrdinalIgnoreCase))
                    return false;

                var departureTime = GetFlightDepartureTime(f);
                return departureTime is not null && departureTime.Value != default && departureTime.Value.Date == transferDate;
            });
        }
    }
}
