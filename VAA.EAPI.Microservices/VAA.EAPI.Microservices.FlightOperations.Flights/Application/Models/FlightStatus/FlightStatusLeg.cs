using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusLeg
    {
        public static explicit operator FlightStatusLeg(DeltaLeg source) => new()
        {
            Id = source.Id,
            MarketingFlightNumber = source.MarketingAirline?.FlightNumber ?? source.FlightNumber,
            Departure = (FlightStatusPoint?)new DeltaDeparturePoint(source),
            Arrival = (FlightStatusPoint?)new DeltaArrivalPoint(source),
            Aircraft = (FlightStatusAircraft?)source.Aircraft,
            Status = (FlightStatusInfo?)source.Status
        };

        public string? Id { get; set; }
        public string? MarketingFlightNumber { get; set; }
        public FlightStatusPoint? Departure { get; set; }
        public FlightStatusPoint? Arrival { get; set; }
        public FlightStatusAircraft? Aircraft { get; set; }
        public FlightStatusInfo? Status { get; set; }
        public FlightStatusLegDetail? Detail { get; set; }
    }
}
