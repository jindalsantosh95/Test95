namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class FlightScheduleRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
