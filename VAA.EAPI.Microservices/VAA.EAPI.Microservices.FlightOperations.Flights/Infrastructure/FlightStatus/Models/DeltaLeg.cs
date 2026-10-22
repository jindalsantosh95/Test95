namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaLeg
    {
        public string? Id { get; set; }
        public string? FlightNumber { get; set; }
        public DeltaAirport? DepartureAirport { get; set; }
        public DeltaAirport? ArrivalAirport { get; set; }
        public DeltaAirline? MarketingAirline { get; set; }
        public DeltaAirline? OperatingAirline { get; set; }
        public DeltaAircraft? Aircraft { get; set; }
        public string? FlightTypeCode { get; set; }
        public bool? ReturnedIndicator { get; set; }
        public string? ShipNumber { get; set; }
        public string? TakeOffNumber { get; set; }
        public bool? Usable { get; set; }
        public DeltaStatus? Status { get; set; }
        public DeltaFlightPositionResponse? FlightPosition { get; set; }
    }
}
