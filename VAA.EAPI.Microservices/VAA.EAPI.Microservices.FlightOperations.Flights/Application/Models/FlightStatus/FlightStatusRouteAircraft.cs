using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteAircraft
    {
        public static explicit operator FlightStatusRouteAircraft?(DeltaAircraft? source) => source is null
            ? null
            : new FlightStatusRouteAircraft
            {
                Type = source.Type,
                DeltaAircraftTypeCode = source.DeltaAircraftTypeCode,
                IndustryStandardAircraftTypeCode = source.IndustryStandardAircraftTypeCode,
                Specification = (FlightStatusRouteAircraftSpecification?)source.Specification
            };

        public string? Type { get; set; }
        public string? DeltaAircraftTypeCode { get; set; }
        public string? IndustryStandardAircraftTypeCode { get; set; }
        public FlightStatusRouteAircraftSpecification? Specification { get; set; }
    }
}
