using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusAirport
    {
        public static explicit operator FlightStatusAirport?(DeltaAirport? source) => source is null
            ? null
            : new FlightStatusAirport
            {
                Code = source.Code,
                Terminal = (FlightStatusTerminal?)source.Terminal,
                Name = source.Name,
                Url = source.Url,
                CityName = source.CityName,
                City = (FlightStatusCity?)source.City,
                Coordinate = (FlightStatusCoordinate?)source.Coordinate
            };

        public string? Code { get; set; }
        public FlightStatusTerminal? Terminal { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? CityName { get; set; }
        public FlightStatusCity? City { get; set; }
        public FlightStatusCoordinate? Coordinate { get; set; }
    }
}
