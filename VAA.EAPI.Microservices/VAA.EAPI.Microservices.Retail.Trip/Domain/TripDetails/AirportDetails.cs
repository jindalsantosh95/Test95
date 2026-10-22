namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class AirportDetails
    {
        public string? TerminalName { get; set; }
        public string? Code { get; set; }
        public string? AirportName { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }

        public static explicit operator AirportDetails(Infrastructure.Proxy.DeltaMyTrips.TransportOrigin source)
        {
            return new AirportDetails
            {
                TerminalName = source?.BoardingTerminal?.Name,
                Code = source?.Station?.Code,
                AirportName = source?.Station?.Name,
                CityName = source?.Station?.PostalAddress?.CityName,
                CountryCode = source?.Station?.PostalAddress?.Country?.Code
            };
        }

        public static explicit operator AirportDetails(Infrastructure.Proxy.DeltaMyTrips.TransportDestination source)
        {
            return new AirportDetails
            {
                TerminalName = source?.DisembarkTerminal?.Name,
                Code = source?.Station?.Code,
                AirportName = source?.Station?.Name,
                CityName = source?.Station?.PostalAddress?.CityName,
                CountryCode = source?.Station?.PostalAddress?.Country?.Code
            };
        }

        public static explicit operator AirportDetails(Infrastructure.Proxy.DeltaMyTrips.OriginStation source)
        {
            return new AirportDetails
            {
                Code = source?.Code,
                AirportName = source?.Name,
                CityName = source?.PostalAddress?.CityName,
                CountryCode = source?.PostalAddress?.Country?.Code
            };
        }

        public static explicit operator AirportDetails(Infrastructure.Proxy.DeltaMyTrips.DestinationStation source)
        {
            return new AirportDetails
            {
                Code = source?.Code,
                AirportName = source?.Name,
                CityName = source?.PostalAddress?.CityName,
                CountryCode = source?.PostalAddress?.Country?.Code
            };
        }
    }
}
