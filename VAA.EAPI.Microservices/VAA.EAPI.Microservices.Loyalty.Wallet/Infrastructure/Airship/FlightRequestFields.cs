namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class FlightRequestFields
    {
        public FlightNumber FlightNumber { get; set; }
        public AirlineCode AirlineCode { get; set; }
        public AirlineName AirlineName { get; set; }
        public Airport DepartureAirport { get; set; }
        public Airport ArrivalAirport { get; set; }
        public FlightSchedule DepartureTime { get; set; }
        public FlightSchedule ArrivalTime { get; set; }
        public FlightSchedule BoardingTime { get; set; }
        public Airport DepartureGate { get; set; }
        public FlightSemantic Semantics { get; set; }
    }
}
