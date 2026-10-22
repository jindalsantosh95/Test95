namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Pos
    {
        public string? CountryCode { get; set; }
        public string? CityCode { get; set; }
        public string? CurrencyCode { get; set; }

        public static explicit operator Pos((Infrastructure.Proxy.DeltaMyTrips.Trip trip, IEnumerable<Infrastructure.Proxy.DeltaMyTrips.Passenger> passengers) source)
        {
            var (trip, passengers) = source;

            return new Pos
            {
                CountryCode = trip.TransportOrigin?.Station?.PostalAddress?.Country?.Code,
                CurrencyCode = passengers.FirstOrDefault()?.Tickets?.FirstOrDefault()?.PosCurrencyCode
            };
        }
    }
}
