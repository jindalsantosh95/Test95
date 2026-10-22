using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusCity
    {
        public static explicit operator FlightStatusCity?(DeltaCity? source) => source is null
            ? null
            : new FlightStatusCity
            {
                Name = source.Name,
                Region = source.Region,
                RegionFullText = source.RegionFullText
            };

        public string? Name { get; set; }
        public string? Region { get; set; }
        public string? RegionFullText { get; set; }
    }
}
