namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class FlightCrewRosterRequest
    {
        public DateOnly FlightDate { get; set; }
        public string Departure { get; set; } = string.Empty;
        public int FlightNo { get; set; } = 0;
    }
}
