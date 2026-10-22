using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteAirline
    {
        public static explicit operator FlightStatusRouteAirline?(DeltaAirline? source) => source is null
            ? null
            : new FlightStatusRouteAirline
            {
                Code = source.Code,
                FlightNumber = source.FlightNumber,
                Name = source.Name
            };

        public string? Code { get; set; }
        public string? FlightNumber { get; set; }
        public string? Name { get; set; }
    }
}
