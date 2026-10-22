namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Sector_VHMMB
    {
        public string? DepartureTime { get; set; }
        public string? ArrivalTime { get; set; }
        public Airport? DepartsFrom { get; set; }
        public Airport? ArrivesAt { get; set; }
        public string? FlightNumber { get; set; }
        public string? CabinClass { get; set; }
        public AirlineDesignator? PreferredAirlineDesignator { get; set; }
        public AirlineDesignator? OperatingAirline { get; set; }
        public AirlineDesignator? MarketingAirline { get; set; }
        public FlightInfoStatusResult? FlightInfoStatusResult { get; set; }
        public string? Duration { get; set; }

    }

}

