namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS
{
    public class FlightSegment
    {
        public int SegmentId { get; set; }
        public int Ordinal { get; set; }
        public IEnumerable<Airline> OperatingAirlines { get; set; }
        public string FlightNumber { get; set; }
        public MarketingFlight MarketingFlight { get; set; }     
        public IEnumerable<Airport> DepartureAirport { get; set; }
        public IEnumerable<Airport> ArrivalAirport { get; set; }
        public bool CheckinStatus { get; set; }
        public IEnumerable<FlightLeg> FlightLegs { get; set; }
    }
}
