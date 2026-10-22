using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusCoordinate
    {
        public static explicit operator FlightStatusCoordinate?(DeltaCoordinate? source) => source is null
            ? null
            : new FlightStatusCoordinate
            {
                Latitude = source.Latitude,
                Longitude = source.Longitude
            };

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
